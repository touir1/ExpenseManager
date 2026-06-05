using Microsoft.Extensions.Options;
using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Infrastructure.Options;
using System.Text.Json;
using Touir.ExpensesManager.Expenses.Messaging.Messages;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Models.Lookups;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;
using Touir.ExpensesManager.Expenses.Repositories.External.Contracts;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Services
{
    public class FamilyService : IFamilyService
    {
        private const string RoleMember = "Member";
        private const string RoleHead = "Head";

        private readonly IFamilyRepository _familyRepo;
        private readonly IUserRepository _userRepo;
        private readonly ILookupCacheService _lookupCache;
        private readonly FamilyOptions _familyOptions;
        private readonly IExpensesOutboxRepository _outboxRepo;

        public FamilyService(IFamilyRepository familyRepo, IUserRepository userRepo, ILookupCacheService lookupCache, IOptions<FamilyOptions> familyOptions, IExpensesOutboxRepository outboxRepo)
        {
            _familyRepo = familyRepo;
            _userRepo = userRepo;
            _lookupCache = lookupCache;
            _familyOptions = familyOptions.Value;
            _outboxRepo = outboxRepo;
        }

        public async Task CreateDefaultAsync(int userId)
        {
            if (await _familyRepo.HasDefaultFamilyAsync(userId))
                return;

            var headId = await _lookupCache.GetIdAsync<FamilyRole>(RoleHead);
            var family = new Family
            {
                Name = "Default",
                IsDefault = true,
                CreatedAt = DateTime.UtcNow,
                CreatedById = userId
            };
            var membership = new FamilyMembership
            {
                UserId = userId,
                RoleId = headId,
                JoinedAt = DateTime.UtcNow
            };

            await _familyRepo.CreateAsync(family, membership);
        }

        public async Task<FamilyDto> CreateAsync(string name, int userId)
        {
            if (await _familyRepo.ExistsWithNameForUserAsync(name, userId))
                throw new FamilyConflictException(ServiceErrors.FamilyNameAlreadyExists);

            var headId = await _lookupCache.GetIdAsync<FamilyRole>(RoleHead);
            var family = new Family
            {
                Name = name,
                IsDefault = false,
                CreatedAt = DateTime.UtcNow,
                CreatedById = userId
            };
            var membership = new FamilyMembership
            {
                UserId = userId,
                RoleId = headId,
                JoinedAt = DateTime.UtcNow
            };

            await _familyRepo.CreateAsync(family, membership);
            return MapToDto(family, RoleHead);
        }

        public async Task<IEnumerable<FamilyDto>> GetByUserAsync(int userId)
        {
            var families = await _familyRepo.GetFamiliesByUserAsync(userId);
            return families.Select(f => MapToDto(f.Family, f.Membership.Role?.Name ?? RoleMember));
        }

        public async Task<FamilyDetailDto> GetByIdAsync(int id, int userId)
        {
            var (family, members) = await _familyRepo.GetByIdWithMembersAsync(id);
            if (family is null)
                throw new FamilyNotFoundException();

            var userMembership = members.FirstOrDefault(m => m.UserId == userId)
                ?? throw new FamilyForbiddenException();

            return new FamilyDetailDto
            {
                Id = family.Id,
                Name = family.Name,
                IsDefault = family.IsDefault,
                IsArchived = family.IsDeleted,
                UserRole = userMembership.Role?.Name ?? RoleMember,
                CreatedAt = family.CreatedAt,
                Members = members.Select(m => new FamilyMemberDto
                {
                    UserId = m.UserId,
                    FirstName = m.User?.FirstName ?? string.Empty,
                    LastName = m.User?.LastName ?? string.Empty,
                    Email = m.User?.Email ?? string.Empty,
                    Role = m.Role?.Name ?? RoleMember,
                    JoinedAt = m.JoinedAt
                })
            };
        }

        public async Task<FamilyDto> RenameAsync(int id, string name, int userId)
        {
            var family = await _familyRepo.GetByIdAsync(id)
                ?? throw new FamilyNotFoundException();

            var membership = await _familyRepo.GetMembershipAsync(id, userId)
                ?? throw new FamilyForbiddenException();

            var headId = await _lookupCache.GetIdAsync<FamilyRole>(RoleHead);
            if (membership.RoleId != headId)
                throw new FamilyForbiddenException();

            if (await _familyRepo.ExistsWithNameForUserAsync(name, userId, excludeId: id))
                throw new FamilyConflictException(ServiceErrors.FamilyNameAlreadyExists);

            family.Name = name;
            await _familyRepo.UpdateFamilyAsync(family);
            return MapToDto(family, membership.Role?.Name ?? RoleHead);
        }

        public async Task<string> InviteAsync(int familyId, string email, int invitedById)
        {
            var family = await _familyRepo.GetByIdAsync(familyId)
                ?? throw new FamilyNotFoundException();

            if (family.IsDefault)
                throw new FamilyForbiddenException(ServiceErrors.FamilyCannotInviteDefault);

            var membership = await _familyRepo.GetMembershipAsync(familyId, invitedById)
                ?? throw new FamilyForbiddenException();

            var headId = await _lookupCache.GetIdAsync<FamilyRole>(RoleHead);
            if (membership.RoleId != headId)
                throw new FamilyForbiddenException();

            var invitee = await _userRepo.GetUserByEmailAsync(email)
                ?? throw new FamilyNotFoundException(ServiceErrors.UserNotFound);

            if (await _familyRepo.IsMemberAsync(familyId, invitee.Id))
                throw new FamilyConflictException(ServiceErrors.FamilyAlreadyMember);

            var token = Guid.NewGuid().ToString();
            var invitation = new FamilyInvitation
            {
                FamilyId = familyId,
                InviteeEmail = email,
                Token = token,
                InvitedById = invitedById,
                InvitedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(_familyOptions.InviteExpiryInDays)
            };

            await _familyRepo.AddInvitationAsync(invitation);

            try
            {
                var inviter = await _userRepo.GetUserByIdAsync(invitedById);
                var inviterName = inviter != null
                    ? $"{inviter.FirstName} {inviter.LastName}".Trim()
                    : string.Empty;

                string inviteLink = $"{_familyOptions.InviteBaseUrl.TrimEnd('/')}?token={Uri.EscapeDataString(token)}";
                var msgId = Guid.NewGuid().ToString();
                await _outboxRepo.EnqueueAsync(new Models.OutboxEvent
                {
                    MessageId = msgId,
                    EventType = FamilyEventType.InvitationRequested,
                    Payload = JsonSerializer.Serialize(new FamilyInvitationEventMessage
                    {
                        MessageId = msgId,
                        EventType = FamilyEventType.InvitationRequested,
                        InviteeEmail = email,
                        InviterName = inviterName,
                        FamilyName = family.Name,
                        InviteLink = inviteLink
                    }),
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }

            return token;
        }

        public async Task AcceptInviteAsync(string token, int userId)
        {
            var invitation = await _familyRepo.GetInvitationByTokenAsync(token)
                ?? throw new FamilyInvitationException(ServiceErrors.FamilyInvitationInvalid);

            if (invitation.AcceptedAt.HasValue)
                throw new FamilyInvitationException(ServiceErrors.FamilyInvitationAlreadyAccepted);

            if (invitation.ExpiresAt < DateTime.UtcNow)
                throw new FamilyInvitationException(ServiceErrors.FamilyInvitationExpired);

            var user = await _userRepo.GetUserByIdAsync(userId)
                ?? throw new FamilyNotFoundException(ServiceErrors.UserNotFound);

            if (!string.Equals(user.Email, invitation.InviteeEmail, StringComparison.OrdinalIgnoreCase))
                throw new FamilyForbiddenException();

            var (family, existingMembers) = await _familyRepo.GetByIdWithMembersAsync(invitation.FamilyId);
            if (family is null)
                throw new FamilyNotFoundException();

            if (family.IsDefault)
                throw new FamilyForbiddenException(ServiceErrors.FamilyCannotInviteDefault);

            if (await _familyRepo.IsMemberAsync(invitation.FamilyId, userId))
                throw new FamilyConflictException(ServiceErrors.FamilyAlreadyMember);

            var headRoleId = await _lookupCache.GetIdAsync<FamilyRole>(RoleHead);
            var memberId = await _lookupCache.GetIdAsync<FamilyRole>(RoleMember);
            var membersList = existingMembers.ToList();

            var membershipEntry = new FamilyMembership
            {
                FamilyId = invitation.FamilyId,
                UserId = userId,
                RoleId = memberId,
                JoinedAt = DateTime.UtcNow
            };
            await _familyRepo.AddMemberAsync(membershipEntry);

            invitation.AcceptedAt = DateTime.UtcNow;
            invitation.AcceptedByUserId = userId;
            await _familyRepo.UpdateInvitationAsync(invitation);

            try
            {
                var joinerName = $"{user.FirstName} {user.LastName}".Trim();
                var headMember = membersList.FirstOrDefault(m => m.RoleId == headRoleId);

                if (headMember is not null)
                {
                    var acceptedMsgId = Guid.NewGuid().ToString();
                    await _outboxRepo.EnqueueAsync(new Models.OutboxEvent
                    {
                        MessageId = acceptedMsgId,
                        EventType = FamilyEventType.InvitationAccepted,
                        Payload = JsonSerializer.Serialize(new FamilyInvitationAcceptedEventMessage
                        {
                            MessageId = acceptedMsgId,
                            EventType = FamilyEventType.InvitationAccepted,
                            HeadUserId = headMember.UserId,
                            FamilyId = invitation.FamilyId,
                            FamilyName = family.Name,
                            AcceptorName = joinerName,
                            AcceptorEmail = user.Email
                        }),
                        CreatedAt = DateTime.UtcNow
                    });
                }

                var existingMemberIds = membersList
                    .Select(m => m.UserId)
                    .Where(id => id != userId)
                    .ToList();

                if (existingMemberIds.Count > 0)
                {
                    var joinedMsgId = Guid.NewGuid().ToString();
                    await _outboxRepo.EnqueueAsync(new Models.OutboxEvent
                    {
                        MessageId = joinedMsgId,
                        EventType = FamilyEventType.MemberJoined,
                        Payload = JsonSerializer.Serialize(new FamilyMemberJoinedEventMessage
                        {
                            MessageId = joinedMsgId,
                            EventType = FamilyEventType.MemberJoined,
                            FamilyId = invitation.FamilyId,
                            FamilyName = family.Name,
                            JoinerName = joinerName,
                            JoinerUserId = userId,
                            MemberUserIds = existingMemberIds
                        }),
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }

        public async Task RemoveMemberAsync(int familyId, int targetUserId, int removedById)
        {
            var family = await _familyRepo.GetByIdAsync(familyId)
                ?? throw new FamilyNotFoundException();

            if (family.IsDefault)
                throw new FamilyForbiddenException(ServiceErrors.FamilyCannotRemoveDefault);

            var removerMembership = await _familyRepo.GetMembershipAsync(familyId, removedById)
                ?? throw new FamilyForbiddenException();

            var headId = await _lookupCache.GetIdAsync<FamilyRole>(RoleHead);
            if (removerMembership.RoleId != headId)
                throw new FamilyForbiddenException();

            var targetMembership = await _familyRepo.GetMembershipAsync(familyId, targetUserId)
                ?? throw new FamilyNotFoundException(ServiceErrors.FamilyNotMember);

            if (targetUserId == removedById)
            {
                var headCount = await _familyRepo.CountHeadsAsync(familyId, headId);
                if (headCount <= 1)
                    throw new FamilyForbiddenException(ServiceErrors.FamilyCannotRemoveSelfHead);
            }

            var expenseCount = await _familyRepo.CountMemberAttributionsAsync(familyId, targetUserId);

            await _familyRepo.RemoveMemberAsync(targetMembership);
            await _familyRepo.RemoveMemberAttributionsAsync(familyId, targetUserId);

            var target = await _userRepo.GetUserByIdAsync(targetUserId);
            var remover = await _userRepo.GetUserByIdAsync(removedById);

            if (target is not null)
            {
                var message = new FamilyEventMessage
                {
                    MessageId = Guid.NewGuid().ToString(),
                    EventType = FamilyEventType.MemberRemoved,
                    TargetUserId = targetUserId,
                    TargetEmail = target.Email,
                    TargetFirstName = target.FirstName ?? string.Empty,
                    FamilyId = familyId,
                    FamilyName = family.Name,
                    RemovedByUserId = removedById,
                    RemovedByName = remover is not null
                        ? $"{remover.FirstName} {remover.LastName}".Trim()
                        : string.Empty,
                    ExpenseCount = expenseCount,
                    OccurredAt = DateTime.UtcNow
                };

                await _outboxRepo.EnqueueAsync(new Models.OutboxEvent
                {
                    MessageId = message.MessageId,
                    EventType = message.EventType,
                    Payload = JsonSerializer.Serialize(message),
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        public async Task ChangeRoleAsync(int familyId, int targetUserId, string roleName, int changedById)
        {
            var changerMembership = await _familyRepo.GetMembershipAsync(familyId, changedById)
                ?? throw new FamilyForbiddenException();

            var headId = await _lookupCache.GetIdAsync<FamilyRole>(RoleHead);
            if (changerMembership.RoleId != headId)
                throw new FamilyForbiddenException();

            if (targetUserId == changedById)
                throw new FamilyForbiddenException(ServiceErrors.FamilyCannotChangeOwnRole);

            var targetMembership = await _familyRepo.GetMembershipAsync(familyId, targetUserId)
                ?? throw new FamilyNotFoundException(ServiceErrors.FamilyNotMember);

            var normalized = char.ToUpperInvariant(roleName[0]) + roleName[1..].ToLowerInvariant();
            targetMembership.RoleId = await _lookupCache.GetIdAsync<FamilyRole>(normalized);
            await _familyRepo.UpdateMemberAsync(targetMembership);
        }

        public async Task ArchiveAsync(int familyId, int userId)
        {
            var family = await _familyRepo.GetByIdAsync(familyId)
                ?? throw new FamilyNotFoundException();

            if (family.IsDefault)
                throw new FamilyForbiddenException(ServiceErrors.FamilyCannotArchiveDefault);

            var membership = await _familyRepo.GetMembershipAsync(familyId, userId)
                ?? throw new FamilyForbiddenException();

            var headId = await _lookupCache.GetIdAsync<FamilyRole>(RoleHead);
            if (membership.RoleId != headId)
                throw new FamilyForbiddenException();

            family.IsDeleted = true;
            family.DeletedAt = DateTime.UtcNow;
            await _familyRepo.UpdateFamilyAsync(family);
        }

        public async Task UnarchiveAsync(int familyId, int userId)
        {
            var family = await _familyRepo.GetByIdAsync(familyId)
                ?? throw new FamilyNotFoundException();

            var membership = await _familyRepo.GetMembershipAsync(familyId, userId)
                ?? throw new FamilyForbiddenException();

            var headId = await _lookupCache.GetIdAsync<FamilyRole>(RoleHead);
            if (membership.RoleId != headId)
                throw new FamilyForbiddenException();

            family.IsDeleted = false;
            family.DeletedAt = null;
            await _familyRepo.UpdateFamilyAsync(family);
        }

        public async Task LeaveAsync(int familyId, int userId)
        {
            var family = await _familyRepo.GetByIdAsync(familyId)
                ?? throw new FamilyNotFoundException();

            if (family.IsDefault)
                throw new FamilyForbiddenException(ServiceErrors.FamilyCannotLeaveDefault);

            var membership = await _familyRepo.GetMembershipAsync(familyId, userId)
                ?? throw new FamilyForbiddenException();

            var headId = await _lookupCache.GetIdAsync<FamilyRole>(RoleHead);
            if (membership.RoleId == headId)
            {
                var headCount = await _familyRepo.CountHeadsAsync(familyId, headId);
                if (headCount <= 1)
                    throw new FamilyForbiddenException(ServiceErrors.FamilyCannotLeaveLastHead);
            }

            await _familyRepo.RemoveMemberAsync(membership);
            await _familyRepo.RemoveMemberAttributionsAsync(familyId, userId);
        }

        private static FamilyDto MapToDto(Family family, string roleName) => new()
        {
            Id = family.Id,
            Name = family.Name,
            IsDefault = family.IsDefault,
            IsArchived = family.IsDeleted,
            UserRole = roleName,
            CreatedAt = family.CreatedAt
        };
    }
}
