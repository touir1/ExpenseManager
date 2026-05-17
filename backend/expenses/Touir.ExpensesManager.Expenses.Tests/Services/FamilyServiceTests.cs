using Microsoft.Extensions.Options;
using Moq;
using Touir.ExpensesManager.Expenses.Infrastructure.Contracts;
using Touir.ExpensesManager.Expenses.Infrastructure.Options;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Models.External;
using Touir.ExpensesManager.Expenses.Models.Lookups;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;
using Touir.ExpensesManager.Expenses.Repositories.External.Contracts;
using Touir.ExpensesManager.Expenses.Services;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Tests.Services
{
    public class FamilyServiceTests
    {
        private static ILookupCacheService DefaultLookupCache()
        {
            var mock = new Mock<ILookupCacheService>();
            mock.Setup(l => l.GetIdAsync<FamilyRole>("Head")).ReturnsAsync(1);
            mock.Setup(l => l.GetIdAsync<FamilyRole>("Member")).ReturnsAsync(2);
            return mock.Object;
        }

        private static FamilyService CreateService(
            IFamilyRepository? familyRepo = null,
            IUserRepository? userRepo = null,
            ILookupCacheService? lookupCache = null,
            IEmailHelper? emailHelper = null,
            IOptions<FamilyOptions>? familyOptions = null)
        {
            return new FamilyService(
                familyRepo ?? Mock.Of<IFamilyRepository>(),
                userRepo ?? Mock.Of<IUserRepository>(),
                lookupCache ?? DefaultLookupCache(),
                emailHelper ?? Mock.Of<IEmailHelper>(),
                familyOptions ?? Options.Create(new FamilyOptions { InviteExpiryInDays = 7 }));
        }

        private static Family MakeFamily(int id = 1, bool isDefault = false, bool isDeleted = false) => new()
        {
            Id = id,
            Name = isDefault ? "Default" : $"Family {id}",
            IsDefault = isDefault,
            IsDeleted = isDeleted,
            CreatedAt = DateTime.UtcNow,
            CreatedById = 1
        };

        private static FamilyMembership MakeMembership(int familyId = 1, int userId = 10, int roleId = 1) => new()
        {
            Id = familyId * 100 + userId,
            FamilyId = familyId,
            UserId = userId,
            RoleId = roleId,
            JoinedAt = DateTime.UtcNow,
            Role = new FamilyRole { Id = roleId, Name = roleId == 1 ? "Head" : "Member" }
        };

        private static User MakeUser(int id = 10, string email = "user@example.com") => new()
        {
            Id = id,
            Email = email,
            FirstName = "Test",
            LastName = "User"
        };

        // ── CreateDefaultAsync ────────────────────────────────────────────────

        [Fact]
        public async Task CreateDefaultAsync_CreatesFamily_WhenNoneExists()
        {
            var repo = new Mock<IFamilyRepository>();
            repo.Setup(r => r.HasDefaultFamilyAsync(10)).ReturnsAsync(false);
            repo.Setup(r => r.CreateAsync(It.IsAny<Family>(), It.IsAny<FamilyMembership>()))
                .ReturnsAsync((Family f, FamilyMembership _) => f);

            await CreateService(repo.Object).CreateDefaultAsync(10);

            repo.Verify(r => r.CreateAsync(
                It.Is<Family>(f => f.IsDefault && f.Name == "Default" && f.CreatedById == 10),
                It.Is<FamilyMembership>(m => m.UserId == 10 && m.RoleId == 1)), Times.Once);
        }

        [Fact]
        public async Task CreateDefaultAsync_IsIdempotent_WhenAlreadyExists()
        {
            var repo = new Mock<IFamilyRepository>();
            repo.Setup(r => r.HasDefaultFamilyAsync(10)).ReturnsAsync(true);

            await CreateService(repo.Object).CreateDefaultAsync(10);

            repo.Verify(r => r.CreateAsync(It.IsAny<Family>(), It.IsAny<FamilyMembership>()), Times.Never);
        }

        // ── CreateAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_CreatesNonDefaultFamily()
        {
            var repo = new Mock<IFamilyRepository>();
            repo.Setup(r => r.CreateAsync(It.IsAny<Family>(), It.IsAny<FamilyMembership>()))
                .ReturnsAsync((Family f, FamilyMembership _) => { f.Id = 5; return f; });

            var result = await CreateService(repo.Object).CreateAsync("My Family", userId: 10);

            Assert.Equal("My Family", result.Name);
            Assert.False(result.IsDefault);
            Assert.Equal("Head", result.UserRole);
        }

        // ── GetByUserAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task GetByUserAsync_ReturnsMappedFamilies()
        {
            var family = MakeFamily(1);
            var membership = MakeMembership(1, 10, roleId: 2);
            membership.Family = family;

            var repo = new Mock<IFamilyRepository>();
            repo.Setup(r => r.GetFamiliesByUserAsync(10))
                .ReturnsAsync(new[] { (family, membership) });

            var results = (await CreateService(repo.Object).GetByUserAsync(10)).ToList();

            Assert.Single(results);
            Assert.Equal(1, results[0].Id);
            Assert.Equal("Member", results[0].UserRole);
        }

        // ── GetByIdAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_ThrowsNotFound_WhenFamilyMissing()
        {
            var repo = new Mock<IFamilyRepository>();
            repo.Setup(r => r.GetByIdWithMembersAsync(99))
                .ReturnsAsync(((Family?)null, Enumerable.Empty<FamilyMembership>()));

            await Assert.ThrowsAsync<FamilyNotFoundException>(
                () => CreateService(repo.Object).GetByIdAsync(99, 10));
        }

        [Fact]
        public async Task GetByIdAsync_ThrowsForbidden_WhenUserNotMember()
        {
            var family = MakeFamily(1);
            var otherMembership = MakeMembership(1, userId: 99);
            otherMembership.User = MakeUser(99);

            var repo = new Mock<IFamilyRepository>();
            repo.Setup(r => r.GetByIdWithMembersAsync(1))
                .ReturnsAsync((family, new[] { otherMembership }.AsEnumerable()));

            await Assert.ThrowsAsync<FamilyForbiddenException>(
                () => CreateService(repo.Object).GetByIdAsync(1, userId: 10));
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsFamilyWithMembers()
        {
            var family = MakeFamily(1);
            var membership = MakeMembership(1, userId: 10);
            membership.User = MakeUser(10);

            var repo = new Mock<IFamilyRepository>();
            repo.Setup(r => r.GetByIdWithMembersAsync(1))
                .ReturnsAsync((family, new[] { membership }.AsEnumerable()));

            var result = await CreateService(repo.Object).GetByIdAsync(1, userId: 10);

            Assert.Equal(1, result.Id);
            Assert.Single(result.Members);
            Assert.Equal(10, result.Members.First().UserId);
        }

        // ── RenameAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task RenameAsync_ThrowsNotFound_WhenFamilyMissing()
        {
            var repo = new Mock<IFamilyRepository>();
            repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Family?)null);

            await Assert.ThrowsAsync<FamilyNotFoundException>(
                () => CreateService(repo.Object).RenameAsync(99, "New Name", userId: 10));
        }

        [Fact]
        public async Task RenameAsync_ThrowsForbidden_WhenNotHead()
        {
            var family = MakeFamily(1);
            var membership = MakeMembership(1, userId: 10, roleId: 2); // Member role

            var repo = new Mock<IFamilyRepository>();
            repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(family);
            repo.Setup(r => r.GetMembershipAsync(1, 10)).ReturnsAsync(membership);

            await Assert.ThrowsAsync<FamilyForbiddenException>(
                () => CreateService(repo.Object).RenameAsync(1, "New Name", userId: 10));
        }

        [Fact]
        public async Task RenameAsync_UpdatesName_WhenHead()
        {
            var family = MakeFamily(1);
            var membership = MakeMembership(1, userId: 10, roleId: 1);

            var repo = new Mock<IFamilyRepository>();
            repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(family);
            repo.Setup(r => r.GetMembershipAsync(1, 10)).ReturnsAsync(membership);

            var result = await CreateService(repo.Object).RenameAsync(1, "New Name", userId: 10);

            repo.Verify(r => r.UpdateFamilyAsync(It.Is<Family>(f => f.Name == "New Name")), Times.Once);
            Assert.Equal("New Name", result.Name);
        }

        // ── ArchiveAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task ArchiveAsync_ThrowsForbidden_WhenDefaultFamily()
        {
            var family = MakeFamily(1, isDefault: true);
            var repo = new Mock<IFamilyRepository>();
            repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(family);

            var ex = await Assert.ThrowsAsync<FamilyForbiddenException>(
                () => CreateService(repo.Object).ArchiveAsync(1, userId: 10));
            Assert.Equal("FAMILY_CANNOT_ARCHIVE_DEFAULT", ex.Message);
        }

        [Fact]
        public async Task ArchiveAsync_SoftDeletes_WhenHead()
        {
            var family = MakeFamily(1);
            var membership = MakeMembership(1, userId: 10, roleId: 1);

            var repo = new Mock<IFamilyRepository>();
            repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(family);
            repo.Setup(r => r.GetMembershipAsync(1, 10)).ReturnsAsync(membership);

            await CreateService(repo.Object).ArchiveAsync(1, userId: 10);

            repo.Verify(r => r.UpdateFamilyAsync(It.Is<Family>(f => f.IsDeleted && f.DeletedAt.HasValue)), Times.Once);
        }

        // ── UnarchiveAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task UnarchiveAsync_ClearsIsDeleted_WhenHead()
        {
            var family = MakeFamily(1, isDeleted: true);
            var membership = MakeMembership(1, userId: 10, roleId: 1);

            var repo = new Mock<IFamilyRepository>();
            repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(family);
            repo.Setup(r => r.GetMembershipAsync(1, 10)).ReturnsAsync(membership);

            await CreateService(repo.Object).UnarchiveAsync(1, userId: 10);

            repo.Verify(r => r.UpdateFamilyAsync(It.Is<Family>(f => !f.IsDeleted && f.DeletedAt == null)), Times.Once);
        }

        // ── InviteAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task InviteAsync_ThrowsForbidden_WhenNotHead()
        {
            var membership = MakeMembership(1, userId: 10, roleId: 2);
            var repo = new Mock<IFamilyRepository>();
            repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeFamily(1));
            repo.Setup(r => r.GetMembershipAsync(1, 10)).ReturnsAsync(membership);

            await Assert.ThrowsAsync<FamilyForbiddenException>(
                () => CreateService(repo.Object).InviteAsync(1, "other@example.com", invitedById: 10));
        }

        [Fact]
        public async Task InviteAsync_ThrowsNotFound_WhenUserNotRegistered()
        {
            var membership = MakeMembership(1, userId: 10, roleId: 1);
            var repo = new Mock<IFamilyRepository>();
            repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeFamily(1));
            repo.Setup(r => r.GetMembershipAsync(1, 10)).ReturnsAsync(membership);

            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("unknown@example.com")).ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<FamilyNotFoundException>(
                () => CreateService(repo.Object, userRepo.Object).InviteAsync(1, "unknown@example.com", invitedById: 10));
        }

        [Fact]
        public async Task InviteAsync_ThrowsConflict_WhenAlreadyMember()
        {
            var membership = MakeMembership(1, userId: 10, roleId: 1);
            var invitee = MakeUser(20, "other@example.com");

            var repo = new Mock<IFamilyRepository>();
            repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeFamily(1));
            repo.Setup(r => r.GetMembershipAsync(1, 10)).ReturnsAsync(membership);
            repo.Setup(r => r.IsMemberAsync(1, 20)).ReturnsAsync(true);

            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("other@example.com")).ReturnsAsync(invitee);

            await Assert.ThrowsAsync<FamilyConflictException>(
                () => CreateService(repo.Object, userRepo.Object).InviteAsync(1, "other@example.com", invitedById: 10));
        }

        [Fact]
        public async Task InviteAsync_ReturnsToken_WhenValid()
        {
            var membership = MakeMembership(1, userId: 10, roleId: 1);
            var invitee = MakeUser(20, "other@example.com");

            var repo = new Mock<IFamilyRepository>();
            repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(MakeFamily(1));
            repo.Setup(r => r.GetMembershipAsync(1, 10)).ReturnsAsync(membership);
            repo.Setup(r => r.IsMemberAsync(1, 20)).ReturnsAsync(false);

            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("other@example.com")).ReturnsAsync(invitee);

            var token = await CreateService(repo.Object, userRepo.Object).InviteAsync(1, "other@example.com", invitedById: 10);

            Assert.NotEmpty(token);
            Assert.Equal(36, token.Length); // GUID format
            repo.Verify(r => r.AddInvitationAsync(It.Is<FamilyInvitation>(i =>
                i.FamilyId == 1 && i.InviteeEmail == "other@example.com" && i.Token == token)), Times.Once);
        }

        [Fact]
        public async Task InviteAsync_SendsEmail_ToInvitee_WhenValid()
        {
            var family = MakeFamily(1);
            var membership = MakeMembership(1, userId: 10, roleId: 1);
            var inviter = MakeUser(10, "inviter@example.com");
            var invitee = MakeUser(20, "other@example.com");

            var repo = new Mock<IFamilyRepository>();
            repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(family);
            repo.Setup(r => r.GetMembershipAsync(1, 10)).ReturnsAsync(membership);
            repo.Setup(r => r.IsMemberAsync(1, 20)).ReturnsAsync(false);

            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("other@example.com")).ReturnsAsync(invitee);
            userRepo.Setup(r => r.GetUserByIdAsync(10)).ReturnsAsync(inviter);

            var emailHelper = new Mock<IEmailHelper>();

            await CreateService(repo.Object, userRepo.Object, emailHelper: emailHelper.Object)
                .InviteAsync(1, "other@example.com", invitedById: 10);

            emailHelper.Verify(e => e.GetEmailTemplate(
                It.IsAny<string>(),
                It.Is<Dictionary<string, string>>(d =>
                    d.ContainsKey("INVITER_NAME") && d["INVITER_NAME"] == "Test User")),
                Times.Once);
            emailHelper.Verify(e => e.SendEmail(
                "other@example.com",
                null, null,
                "[Expenses Manager] Family Invitation",
                It.IsAny<string?>(),
                true,
                null), Times.Once);
        }

        [Fact]
        public async Task InviteAsync_EmailFailure_DoesNotPropagate()
        {
            var family = MakeFamily(1);
            var membership = MakeMembership(1, userId: 10, roleId: 1);
            var invitee = MakeUser(20, "other@example.com");

            var repo = new Mock<IFamilyRepository>();
            repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(family);
            repo.Setup(r => r.GetMembershipAsync(1, 10)).ReturnsAsync(membership);
            repo.Setup(r => r.IsMemberAsync(1, 20)).ReturnsAsync(false);

            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("other@example.com")).ReturnsAsync(invitee);

            var emailHelper = new Mock<IEmailHelper>();
            emailHelper.Setup(e => e.GetEmailTemplate(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .Throws(new Exception("SMTP down"));

            var token = await CreateService(repo.Object, userRepo.Object, emailHelper: emailHelper.Object)
                .InviteAsync(1, "other@example.com", invitedById: 10);

            Assert.NotEmpty(token);
            repo.Verify(r => r.AddInvitationAsync(It.IsAny<FamilyInvitation>()), Times.Once);
        }

        // ── AcceptInviteAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task AcceptInviteAsync_ThrowsInvitationInvalid_WhenTokenNotFound()
        {
            var repo = new Mock<IFamilyRepository>();
            repo.Setup(r => r.GetInvitationByTokenAsync("bad-token")).ReturnsAsync((FamilyInvitation?)null);

            await Assert.ThrowsAsync<FamilyInvitationException>(
                () => CreateService(repo.Object).AcceptInviteAsync("bad-token", userId: 10));
        }

        [Fact]
        public async Task AcceptInviteAsync_ThrowsAlreadyAccepted_WhenInvitationUsed()
        {
            var invitation = new FamilyInvitation
            {
                Id = 1, FamilyId = 1, InviteeEmail = "user@example.com",
                Token = "tok", InvitedById = 5,
                InvitedAt = DateTime.UtcNow.AddDays(-1),
                ExpiresAt = DateTime.UtcNow.AddDays(6),
                AcceptedAt = DateTime.UtcNow.AddHours(-1)
            };

            var repo = new Mock<IFamilyRepository>();
            repo.Setup(r => r.GetInvitationByTokenAsync("tok")).ReturnsAsync(invitation);

            var ex = await Assert.ThrowsAsync<FamilyInvitationException>(
                () => CreateService(repo.Object).AcceptInviteAsync("tok", userId: 10));
            Assert.Equal("FAMILY_INVITATION_ALREADY_ACCEPTED", ex.Message);
        }

        [Fact]
        public async Task AcceptInviteAsync_ThrowsExpired_WhenExpired()
        {
            var invitation = new FamilyInvitation
            {
                Id = 1, FamilyId = 1, InviteeEmail = "user@example.com",
                Token = "tok", InvitedById = 5,
                InvitedAt = DateTime.UtcNow.AddDays(-8),
                ExpiresAt = DateTime.UtcNow.AddDays(-1)
            };

            var repo = new Mock<IFamilyRepository>();
            repo.Setup(r => r.GetInvitationByTokenAsync("tok")).ReturnsAsync(invitation);

            var ex = await Assert.ThrowsAsync<FamilyInvitationException>(
                () => CreateService(repo.Object).AcceptInviteAsync("tok", userId: 10));
            Assert.Equal("FAMILY_INVITATION_EXPIRED", ex.Message);
        }

        [Fact]
        public async Task AcceptInviteAsync_CreatesMembership_WhenValid()
        {
            var invitation = new FamilyInvitation
            {
                Id = 1, FamilyId = 1, InviteeEmail = "user@example.com",
                Token = "tok", InvitedById = 5,
                InvitedAt = DateTime.UtcNow.AddDays(-1),
                ExpiresAt = DateTime.UtcNow.AddDays(6)
            };
            var user = MakeUser(10, "user@example.com");

            var repo = new Mock<IFamilyRepository>();
            repo.Setup(r => r.GetInvitationByTokenAsync("tok")).ReturnsAsync(invitation);
            repo.Setup(r => r.IsMemberAsync(1, 10)).ReturnsAsync(false);

            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByIdAsync(10)).ReturnsAsync(user);

            await CreateService(repo.Object, userRepo.Object).AcceptInviteAsync("tok", userId: 10);

            repo.Verify(r => r.AddMemberAsync(It.Is<FamilyMembership>(m =>
                m.FamilyId == 1 && m.UserId == 10 && m.RoleId == 2)), Times.Once);
            repo.Verify(r => r.UpdateInvitationAsync(It.Is<FamilyInvitation>(i =>
                i.AcceptedAt.HasValue && i.AcceptedByUserId == 10)), Times.Once);
        }

        // ── RemoveMemberAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task RemoveMemberAsync_ThrowsForbidden_WhenNotHead()
        {
            var membership = MakeMembership(1, userId: 10, roleId: 2);
            var repo = new Mock<IFamilyRepository>();
            repo.Setup(r => r.GetMembershipAsync(1, 10)).ReturnsAsync(membership);

            await Assert.ThrowsAsync<FamilyForbiddenException>(
                () => CreateService(repo.Object).RemoveMemberAsync(1, targetUserId: 20, removedById: 10));
        }

        [Fact]
        public async Task RemoveMemberAsync_ThrowsForbidden_WhenRemovingSelf_AndNoOtherHead()
        {
            var headMembership = MakeMembership(1, userId: 10, roleId: 1);
            var repo = new Mock<IFamilyRepository>();
            repo.Setup(r => r.GetMembershipAsync(1, 10)).ReturnsAsync(headMembership);
            repo.Setup(r => r.CountHeadsAsync(1, 1)).ReturnsAsync(1);

            await Assert.ThrowsAsync<FamilyForbiddenException>(
                () => CreateService(repo.Object).RemoveMemberAsync(1, targetUserId: 10, removedById: 10));
        }

        [Fact]
        public async Task RemoveMemberAsync_AllowsSelfRemoval_WhenOtherHeadExists()
        {
            var headMembership = MakeMembership(1, userId: 10, roleId: 1);
            var repo = new Mock<IFamilyRepository>();
            repo.Setup(r => r.GetMembershipAsync(1, 10)).ReturnsAsync(headMembership);
            repo.Setup(r => r.CountHeadsAsync(1, 1)).ReturnsAsync(2);

            await CreateService(repo.Object).RemoveMemberAsync(1, targetUserId: 10, removedById: 10);

            repo.Verify(r => r.RemoveMemberAsync(headMembership), Times.Once);
            repo.Verify(r => r.RemoveMemberAttributionsAsync(1, 10), Times.Once);
        }

        [Fact]
        public async Task RemoveMemberAsync_RemovesMemberAndAttributions()
        {
            var headMembership = MakeMembership(1, userId: 10, roleId: 1);
            var targetMembership = MakeMembership(1, userId: 20, roleId: 2);

            var repo = new Mock<IFamilyRepository>();
            repo.SetupSequence(r => r.GetMembershipAsync(1, 10))
                .ReturnsAsync(headMembership);
            repo.SetupSequence(r => r.GetMembershipAsync(1, 20))
                .ReturnsAsync(targetMembership);

            await CreateService(repo.Object).RemoveMemberAsync(1, targetUserId: 20, removedById: 10);

            repo.Verify(r => r.RemoveMemberAsync(targetMembership), Times.Once);
            repo.Verify(r => r.RemoveMemberAttributionsAsync(1, 20), Times.Once);
        }

        // ── LeaveAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task LeaveAsync_ThrowsNotFound_WhenFamilyMissing()
        {
            var repo = new Mock<IFamilyRepository>();
            repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Family?)null);

            await Assert.ThrowsAsync<FamilyNotFoundException>(
                () => CreateService(repo.Object).LeaveAsync(1, userId: 10));
        }

        [Fact]
        public async Task LeaveAsync_ThrowsForbidden_WhenDefaultFamily()
        {
            var family = MakeFamily(1, isDefault: true);
            var repo = new Mock<IFamilyRepository>();
            repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(family);

            await Assert.ThrowsAsync<FamilyForbiddenException>(
                () => CreateService(repo.Object).LeaveAsync(1, userId: 10));
        }

        [Fact]
        public async Task LeaveAsync_ThrowsForbidden_WhenNotMember()
        {
            var family = MakeFamily(1);
            var repo = new Mock<IFamilyRepository>();
            repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(family);
            repo.Setup(r => r.GetMembershipAsync(1, 10)).ReturnsAsync((FamilyMembership?)null);

            await Assert.ThrowsAsync<FamilyForbiddenException>(
                () => CreateService(repo.Object).LeaveAsync(1, userId: 10));
        }

        [Fact]
        public async Task LeaveAsync_ThrowsForbidden_WhenLastHead()
        {
            var family = MakeFamily(1);
            var membership = MakeMembership(1, userId: 10, roleId: 1);
            var repo = new Mock<IFamilyRepository>();
            repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(family);
            repo.Setup(r => r.GetMembershipAsync(1, 10)).ReturnsAsync(membership);
            repo.Setup(r => r.CountHeadsAsync(1, 1)).ReturnsAsync(1);

            await Assert.ThrowsAsync<FamilyForbiddenException>(
                () => CreateService(repo.Object).LeaveAsync(1, userId: 10));
        }

        [Fact]
        public async Task LeaveAsync_Succeeds_WhenHeadWithOtherHead()
        {
            var family = MakeFamily(1);
            var membership = MakeMembership(1, userId: 10, roleId: 1);
            var repo = new Mock<IFamilyRepository>();
            repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(family);
            repo.Setup(r => r.GetMembershipAsync(1, 10)).ReturnsAsync(membership);
            repo.Setup(r => r.CountHeadsAsync(1, 1)).ReturnsAsync(2);

            await CreateService(repo.Object).LeaveAsync(1, userId: 10);

            repo.Verify(r => r.RemoveMemberAsync(membership), Times.Once);
            repo.Verify(r => r.RemoveMemberAttributionsAsync(1, 10), Times.Once);
        }

        [Fact]
        public async Task LeaveAsync_Succeeds_ForMember()
        {
            var family = MakeFamily(1);
            var membership = MakeMembership(1, userId: 10, roleId: 2);
            var repo = new Mock<IFamilyRepository>();
            repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(family);
            repo.Setup(r => r.GetMembershipAsync(1, 10)).ReturnsAsync(membership);

            await CreateService(repo.Object).LeaveAsync(1, userId: 10);

            repo.Verify(r => r.RemoveMemberAsync(membership), Times.Once);
            repo.Verify(r => r.RemoveMemberAttributionsAsync(1, 10), Times.Once);
        }

        // ── ChangeRoleAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task ChangeRoleAsync_ThrowsForbidden_WhenChangingOwnRole()
        {
            var headMembership = MakeMembership(1, userId: 10, roleId: 1);
            var repo = new Mock<IFamilyRepository>();
            repo.Setup(r => r.GetMembershipAsync(1, 10)).ReturnsAsync(headMembership);

            await Assert.ThrowsAsync<FamilyForbiddenException>(
                () => CreateService(repo.Object).ChangeRoleAsync(1, targetUserId: 10, roleName: "Member", changedById: 10));
        }

        [Fact]
        public async Task ChangeRoleAsync_ThrowsForbidden_WhenNotHead()
        {
            var membership = MakeMembership(1, userId: 10, roleId: 2);
            var repo = new Mock<IFamilyRepository>();
            repo.Setup(r => r.GetMembershipAsync(1, 10)).ReturnsAsync(membership);

            await Assert.ThrowsAsync<FamilyForbiddenException>(
                () => CreateService(repo.Object).ChangeRoleAsync(1, targetUserId: 20, roleName: "Member", changedById: 10));
        }

        [Fact]
        public async Task ChangeRoleAsync_UpdatesRole_WhenHead()
        {
            var changerMembership = MakeMembership(1, userId: 10, roleId: 1);
            var targetMembership = MakeMembership(1, userId: 20, roleId: 2);

            var repo = new Mock<IFamilyRepository>();
            repo.Setup(r => r.GetMembershipAsync(1, 10)).ReturnsAsync(changerMembership);
            repo.Setup(r => r.GetMembershipAsync(1, 20)).ReturnsAsync(targetMembership);

            await CreateService(repo.Object).ChangeRoleAsync(1, targetUserId: 20, roleName: "Head", changedById: 10);

            repo.Verify(r => r.UpdateMemberAsync(It.Is<FamilyMembership>(m =>
                m.UserId == 20 && m.RoleId == 1)), Times.Once);
        }
    }
}
