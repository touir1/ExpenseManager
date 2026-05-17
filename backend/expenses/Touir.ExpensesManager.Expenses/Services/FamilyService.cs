using Microsoft.Extensions.Options;
using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Infrastructure.Options;
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

        public FamilyService(IFamilyRepository familyRepo, IUserRepository userRepo, ILookupCacheService lookupCache, IOptions<FamilyOptions> familyOptions)
        {
            _familyRepo = familyRepo;
            _userRepo = userRepo;
            _lookupCache = lookupCache;
            _familyOptions = familyOptions.Value;
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

            family.Name = name;
            await _familyRepo.UpdateFamilyAsync(family);
            return MapToDto(family, membership.Role?.Name ?? RoleHead);
        }

        public async Task<string> InviteAsync(int familyId, string email, int invitedById)
        {
            var membership = await _familyRepo.GetMembershipAsync(familyId, invitedById)
                ?? throw new FamilyForbiddenException();

            var headId = await _lookupCache.GetIdAsync<FamilyRole>(RoleHead);
            if (membership.RoleId != headId)
                throw new FamilyForbiddenException();

            var invitee = await _userRepo.GetUserByEmailAsync(email)
                ?? throw new FamilyNotFoundException("USER_NOT_FOUND");

            if (await _familyRepo.IsMemberAsync(familyId, invitee.Id))
                throw new FamilyConflictException("FAMILY_ALREADY_MEMBER");

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
            return token;
        }

        public async Task AcceptInviteAsync(string token, int userId)
        {
            var invitation = await _familyRepo.GetInvitationByTokenAsync(token)
                ?? throw new FamilyInvitationException("FAMILY_INVITATION_INVALID");

            if (invitation.AcceptedAt.HasValue)
                throw new FamilyInvitationException("FAMILY_INVITATION_ALREADY_ACCEPTED");

            if (invitation.ExpiresAt < DateTime.UtcNow)
                throw new FamilyInvitationException("FAMILY_INVITATION_EXPIRED");

            var user = await _userRepo.GetUserByIdAsync(userId)
                ?? throw new FamilyNotFoundException("USER_NOT_FOUND");

            if (!string.Equals(user.Email, invitation.InviteeEmail, StringComparison.OrdinalIgnoreCase))
                throw new FamilyForbiddenException();

            if (await _familyRepo.IsMemberAsync(invitation.FamilyId, userId))
                throw new FamilyConflictException("FAMILY_ALREADY_MEMBER");

            var memberId = await _lookupCache.GetIdAsync<FamilyRole>(RoleMember);
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
        }

        public async Task RemoveMemberAsync(int familyId, int targetUserId, int removedById)
        {
            var removerMembership = await _familyRepo.GetMembershipAsync(familyId, removedById)
                ?? throw new FamilyForbiddenException();

            var headId = await _lookupCache.GetIdAsync<FamilyRole>(RoleHead);
            if (removerMembership.RoleId != headId)
                throw new FamilyForbiddenException();

            var targetMembership = await _familyRepo.GetMembershipAsync(familyId, targetUserId)
                ?? throw new FamilyNotFoundException("FAMILY_NOT_MEMBER");

            if (targetUserId == removedById)
            {
                var headCount = await _familyRepo.CountHeadsAsync(familyId, headId);
                if (headCount <= 1)
                    throw new FamilyForbiddenException("FAMILY_CANNOT_REMOVE_SELF_HEAD");
            }

            await _familyRepo.RemoveMemberAsync(targetMembership);
            await _familyRepo.RemoveMemberAttributionsAsync(familyId, targetUserId);
        }

        public async Task ChangeRoleAsync(int familyId, int targetUserId, string roleName, int changedById)
        {
            var changerMembership = await _familyRepo.GetMembershipAsync(familyId, changedById)
                ?? throw new FamilyForbiddenException();

            var headId = await _lookupCache.GetIdAsync<FamilyRole>(RoleHead);
            if (changerMembership.RoleId != headId)
                throw new FamilyForbiddenException();

            if (targetUserId == changedById)
                throw new FamilyForbiddenException("FAMILY_CANNOT_CHANGE_OWN_ROLE");

            var targetMembership = await _familyRepo.GetMembershipAsync(familyId, targetUserId)
                ?? throw new FamilyNotFoundException("FAMILY_NOT_MEMBER");

            var normalized = char.ToUpperInvariant(roleName[0]) + roleName[1..].ToLowerInvariant();
            targetMembership.RoleId = await _lookupCache.GetIdAsync<FamilyRole>(normalized);
            await _familyRepo.UpdateMemberAsync(targetMembership);
        }

        public async Task ArchiveAsync(int familyId, int userId)
        {
            var family = await _familyRepo.GetByIdAsync(familyId)
                ?? throw new FamilyNotFoundException();

            if (family.IsDefault)
                throw new FamilyForbiddenException("FAMILY_CANNOT_ARCHIVE_DEFAULT");

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
