using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Models.External;
using Touir.ExpensesManager.Expenses.Repositories;
using Touir.ExpensesManager.Expenses.Tests.TestHelpers;

namespace Touir.ExpensesManager.Expenses.Tests.Repositories
{
    public class FamilyRepositoryTests : IDisposable
    {
        private readonly TestExpensesDbContextWrapper _wrapper;
        private readonly FamilyRepository _sut;

        public FamilyRepositoryTests()
        {
            _wrapper = new TestExpensesDbContextWrapper();
            _sut = new FamilyRepository(_wrapper.Context);
        }

        public void Dispose()
        {
            _wrapper.Dispose();
            GC.SuppressFinalize(this);
        }

        private async Task SeedUserAsync(int id = 1, string email = "u@test.com")
        {
            if (!_wrapper.Context.Users.Any(u => u.Id == id))
            {
                _wrapper.Context.Users.Add(new User { Id = id, FirstName = "T", LastName = "U", Email = email, IsDeleted = false });
                await _wrapper.Context.SaveChangesAsync();
            }
        }

        private async Task<Family> SeedFamilyAsync(int createdById = 1, bool isDefault = false)
        {
            await SeedUserAsync(createdById);
            var family = new Family
            {
                Name = isDefault ? "Default" : "Test Family",
                IsDefault = isDefault,
                CreatedAt = DateTime.UtcNow,
                CreatedById = createdById
            };
            _wrapper.Context.Families.Add(family);
            await _wrapper.Context.SaveChangesAsync();
            return family;
        }

        private async Task<FamilyMembership> SeedMembershipAsync(int familyId, int userId, int roleId = 1)
        {
            await SeedUserAsync(userId, $"u{userId}@test.com");
            var membership = new FamilyMembership
            {
                FamilyId = familyId,
                UserId = userId,
                RoleId = roleId,
                JoinedAt = DateTime.UtcNow
            };
            _wrapper.Context.FamilyMemberships.Add(membership);
            await _wrapper.Context.SaveChangesAsync();
            return membership;
        }

        // ── CreateAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_PersistsFamilyAndMembership()
        {
            await SeedUserAsync(1);
            var family = new Family { Name = "F1", IsDefault = false, CreatedAt = DateTime.UtcNow, CreatedById = 1 };
            var membership = new FamilyMembership { UserId = 1, RoleId = 1, JoinedAt = DateTime.UtcNow };

            var result = await _sut.CreateAsync(family, membership);

            Assert.True(result.Id > 0);
            Assert.True(_wrapper.Context.FamilyMemberships.Any(m => m.FamilyId == result.Id && m.UserId == 1));
        }

        // ── GetByIdAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_ReturnsFamily_WhenExists()
        {
            var family = await SeedFamilyAsync();

            var result = await _sut.GetByIdAsync(family.Id);

            Assert.NotNull(result);
            Assert.Equal(family.Id, result!.Id);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
        {
            var result = await _sut.GetByIdAsync(9999);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsSoftDeletedFamily()
        {
            var family = await SeedFamilyAsync();
            family.IsDeleted = true;
            _wrapper.Context.Families.Update(family);
            await _wrapper.Context.SaveChangesAsync();

            var result = await _sut.GetByIdAsync(family.Id);

            Assert.NotNull(result);
            Assert.True(result!.IsDeleted);
        }

        // ── GetByIdWithMembersAsync ───────────────────────────────────────────

        [Fact]
        public async Task GetByIdWithMembersAsync_ReturnsFamilyAndMembers()
        {
            var family = await SeedFamilyAsync(createdById: 1);
            await SeedMembershipAsync(family.Id, userId: 1, roleId: 1);
            await SeedMembershipAsync(family.Id, userId: 2, roleId: 2);

            var (f, members) = await _sut.GetByIdWithMembersAsync(family.Id);

            Assert.NotNull(f);
            Assert.Equal(2, members.Count());
        }

        [Fact]
        public async Task GetByIdWithMembersAsync_ReturnsNullFamily_WhenNotFound()
        {
            var (f, members) = await _sut.GetByIdWithMembersAsync(9999);
            Assert.Null(f);
            Assert.Empty(members);
        }

        // ── GetFamiliesByUserAsync ────────────────────────────────────────────

        [Fact]
        public async Task GetFamiliesByUserAsync_ReturnsUserFamilies()
        {
            var f1 = await SeedFamilyAsync(createdById: 1);
            var f2 = await SeedFamilyAsync(createdById: 1);
            await SeedMembershipAsync(f1.Id, userId: 1, roleId: 1);
            await SeedMembershipAsync(f2.Id, userId: 1, roleId: 2);

            var results = (await _sut.GetFamiliesByUserAsync(1)).ToList();

            Assert.Equal(2, results.Count);
        }

        [Fact]
        public async Task GetFamiliesByUserAsync_ReturnsEmpty_WhenNoMemberships()
        {
            await SeedUserAsync(5);
            var results = await _sut.GetFamiliesByUserAsync(5);
            Assert.Empty(results);
        }

        // ── GetDefaultFamilyForUserAsync ──────────────────────────────────────

        [Fact]
        public async Task GetDefaultFamilyForUserAsync_ReturnsDefault_WhenExists()
        {
            var defaultFamily = await SeedFamilyAsync(createdById: 1, isDefault: true);
            await SeedMembershipAsync(defaultFamily.Id, userId: 1, roleId: 1);

            var result = await _sut.GetDefaultFamilyForUserAsync(1);

            Assert.NotNull(result);
            Assert.True(result!.IsDefault);
        }

        [Fact]
        public async Task GetDefaultFamilyForUserAsync_ReturnsNull_WhenNone()
        {
            await SeedUserAsync(7);
            var result = await _sut.GetDefaultFamilyForUserAsync(7);
            Assert.Null(result);
        }

        // ── HasDefaultFamilyAsync ─────────────────────────────────────────────

        [Fact]
        public async Task HasDefaultFamilyAsync_ReturnsTrue_WhenDefaultExists()
        {
            var defaultFamily = await SeedFamilyAsync(createdById: 1, isDefault: true);
            await SeedMembershipAsync(defaultFamily.Id, userId: 1, roleId: 1);

            Assert.True(await _sut.HasDefaultFamilyAsync(1));
        }

        [Fact]
        public async Task HasDefaultFamilyAsync_ReturnsFalse_WhenNone()
        {
            await SeedUserAsync(8);
            Assert.False(await _sut.HasDefaultFamilyAsync(8));
        }

        // ── GetMembershipAsync ────────────────────────────────────────────────

        [Fact]
        public async Task GetMembershipAsync_ReturnsMembership_WhenExists()
        {
            var family = await SeedFamilyAsync(createdById: 1);
            await SeedMembershipAsync(family.Id, userId: 1, roleId: 1);

            var result = await _sut.GetMembershipAsync(family.Id, 1);

            Assert.NotNull(result);
            Assert.Equal(1, result!.RoleId);
        }

        [Fact]
        public async Task GetMembershipAsync_ReturnsNull_WhenNotMember()
        {
            var family = await SeedFamilyAsync(createdById: 1);
            await SeedUserAsync(9);

            var result = await _sut.GetMembershipAsync(family.Id, 9);
            Assert.Null(result);
        }

        // ── IsMemberAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task IsMemberAsync_ReturnsTrue_WhenMember()
        {
            var family = await SeedFamilyAsync(createdById: 1);
            await SeedMembershipAsync(family.Id, userId: 1);

            Assert.True(await _sut.IsMemberAsync(family.Id, 1));
        }

        [Fact]
        public async Task IsMemberAsync_ReturnsFalse_WhenNotMember()
        {
            var family = await SeedFamilyAsync(createdById: 1);
            await SeedUserAsync(10);

            Assert.False(await _sut.IsMemberAsync(family.Id, 10));
        }

        // ── AddMemberAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task AddMemberAsync_PersistsMembership()
        {
            var family = await SeedFamilyAsync(createdById: 1);
            await SeedUserAsync(2);
            var membership = new FamilyMembership { FamilyId = family.Id, UserId = 2, RoleId = 2, JoinedAt = DateTime.UtcNow };

            await _sut.AddMemberAsync(membership);

            Assert.True(_wrapper.Context.FamilyMemberships.Any(m => m.FamilyId == family.Id && m.UserId == 2));
        }

        // ── UpdateMemberAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task UpdateMemberAsync_PersistsRoleChange()
        {
            var family = await SeedFamilyAsync(createdById: 1);
            var membership = await SeedMembershipAsync(family.Id, userId: 1, roleId: 2);

            membership.RoleId = 1;
            await _sut.UpdateMemberAsync(membership);

            var updated = _wrapper.Context.FamilyMemberships.First(m => m.Id == membership.Id);
            Assert.Equal(1, updated.RoleId);
        }

        // ── RemoveMemberAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task RemoveMemberAsync_DeletesMembership()
        {
            var family = await SeedFamilyAsync(createdById: 1);
            var membership = await SeedMembershipAsync(family.Id, userId: 1, roleId: 1);

            await _sut.RemoveMemberAsync(membership);

            Assert.False(_wrapper.Context.FamilyMemberships.Any(m => m.Id == membership.Id));
        }

        // ── UpdateFamilyAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task UpdateFamilyAsync_PersistsChanges()
        {
            var family = await SeedFamilyAsync(createdById: 1);
            family.Name = "Renamed";

            await _sut.UpdateFamilyAsync(family);

            var updated = _wrapper.Context.Families.First(f => f.Id == family.Id);
            Assert.Equal("Renamed", updated.Name);
        }

        // ── AddInvitationAsync ────────────────────────────────────────────────

        [Fact]
        public async Task AddInvitationAsync_PersistsInvitation()
        {
            var family = await SeedFamilyAsync(createdById: 1);
            var invitation = new FamilyInvitation
            {
                FamilyId = family.Id,
                InviteeEmail = "invite@test.com",
                Token = Guid.NewGuid().ToString(),
                InvitedById = 1,
                InvitedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            await _sut.AddInvitationAsync(invitation);

            Assert.True(_wrapper.Context.FamilyInvitations.Any(i => i.InviteeEmail == "invite@test.com"));
        }

        // ── GetInvitationByTokenAsync ─────────────────────────────────────────

        [Fact]
        public async Task GetInvitationByTokenAsync_ReturnsInvitation_WhenFound()
        {
            var family = await SeedFamilyAsync(createdById: 1);
            var token = Guid.NewGuid().ToString();
            var invitation = new FamilyInvitation
            {
                FamilyId = family.Id,
                InviteeEmail = "x@test.com",
                Token = token,
                InvitedById = 1,
                InvitedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };
            _wrapper.Context.FamilyInvitations.Add(invitation);
            await _wrapper.Context.SaveChangesAsync();

            var result = await _sut.GetInvitationByTokenAsync(token);

            Assert.NotNull(result);
            Assert.Equal(token, result!.Token);
        }

        [Fact]
        public async Task GetInvitationByTokenAsync_ReturnsNull_WhenNotFound()
        {
            var result = await _sut.GetInvitationByTokenAsync("not-a-real-token");
            Assert.Null(result);
        }

        // ── UpdateInvitationAsync ─────────────────────────────────────────────

        [Fact]
        public async Task UpdateInvitationAsync_PersistsAcceptedAt()
        {
            var family = await SeedFamilyAsync(createdById: 1);
            var invitation = new FamilyInvitation
            {
                FamilyId = family.Id,
                InviteeEmail = "x@test.com",
                Token = Guid.NewGuid().ToString(),
                InvitedById = 1,
                InvitedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };
            _wrapper.Context.FamilyInvitations.Add(invitation);
            await _wrapper.Context.SaveChangesAsync();

            invitation.AcceptedAt = DateTime.UtcNow;
            invitation.AcceptedByUserId = 1;
            await _sut.UpdateInvitationAsync(invitation);

            var updated = _wrapper.Context.FamilyInvitations.First(i => i.Id == invitation.Id);
            Assert.True(updated.AcceptedAt.HasValue);
        }

        // ── AddAttributionsAsync ──────────────────────────────────────────────

        [Fact]
        public async Task AddAttributionsAsync_PersistsAttributions()
        {
            await SeedUserAsync(1);
            var family = await SeedFamilyAsync(createdById: 1);

            _wrapper.Context.Currencies.Add(new Models.Currency { Id = 2000, Code = "AT", Name = "A", Symbol = "A", Decimals = 2 });
            await _wrapper.Context.SaveChangesAsync();

            var expense = new Models.Expense
            {
                UserId = 1, Amount = 10m, CurrencyId = 2000,
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                CreatedAt = DateTime.UtcNow, CreatedById = 1, CreatedFromId = 1
            };
            _wrapper.Context.Expenses.Add(expense);
            await _wrapper.Context.SaveChangesAsync();

            var attributions = new[] { new Models.ExpenseFamilyAttribution
            {
                ExpenseId = expense.Id, FamilyId = family.Id,
                AttributedAt = DateTime.UtcNow, AttributedById = 1
            }};

            await _sut.AddAttributionsAsync(attributions);

            Assert.True(_wrapper.Context.ExpenseFamilyAttributions.Any(a => a.ExpenseId == expense.Id));
        }

        // ── ClearAttributionsAsync ────────────────────────────────────────────

        [Fact]
        public async Task ClearAttributionsAsync_RemovesAllForExpense()
        {
            await SeedUserAsync(1);
            var family = await SeedFamilyAsync(createdById: 1);

            _wrapper.Context.Currencies.Add(new Models.Currency { Id = 2001, Code = "CL", Name = "C", Symbol = "C", Decimals = 2 });
            await _wrapper.Context.SaveChangesAsync();

            var expense = new Models.Expense
            {
                UserId = 1, Amount = 10m, CurrencyId = 2001,
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                CreatedAt = DateTime.UtcNow, CreatedById = 1, CreatedFromId = 1
            };
            _wrapper.Context.Expenses.Add(expense);
            await _wrapper.Context.SaveChangesAsync();

            _wrapper.Context.ExpenseFamilyAttributions.Add(new Models.ExpenseFamilyAttribution
            {
                ExpenseId = expense.Id, FamilyId = family.Id,
                AttributedAt = DateTime.UtcNow, AttributedById = 1
            });
            await _wrapper.Context.SaveChangesAsync();

            await _sut.ClearAttributionsAsync(expense.Id);

            Assert.False(_wrapper.Context.ExpenseFamilyAttributions.Any(a => a.ExpenseId == expense.Id));
        }

        // ── RemoveMemberAttributionsAsync ─────────────────────────────────────

        [Fact]
        public async Task RemoveMemberAttributionsAsync_RemovesOnlyTargetOwnerAttributions()
        {
            await SeedUserAsync(1);
            await SeedUserAsync(2, "u2@test.com");
            var family = await SeedFamilyAsync(createdById: 1);

            _wrapper.Context.Currencies.Add(new Models.Currency { Id = 2002, Code = "RM", Name = "R", Symbol = "R", Decimals = 2 });
            await _wrapper.Context.SaveChangesAsync();

            var expenseU1 = new Models.Expense
            {
                UserId = 1, Amount = 10m, CurrencyId = 2002,
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                CreatedAt = DateTime.UtcNow, CreatedById = 1, CreatedFromId = 1
            };
            var expenseU2 = new Models.Expense
            {
                UserId = 2, Amount = 20m, CurrencyId = 2002,
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                CreatedAt = DateTime.UtcNow, CreatedById = 2, CreatedFromId = 1
            };
            _wrapper.Context.Expenses.AddRange(expenseU1, expenseU2);
            await _wrapper.Context.SaveChangesAsync();

            _wrapper.Context.ExpenseFamilyAttributions.AddRange(
                new Models.ExpenseFamilyAttribution { ExpenseId = expenseU1.Id, FamilyId = family.Id, AttributedAt = DateTime.UtcNow, AttributedById = 1 },
                new Models.ExpenseFamilyAttribution { ExpenseId = expenseU2.Id, FamilyId = family.Id, AttributedAt = DateTime.UtcNow, AttributedById = 2 }
            );
            await _wrapper.Context.SaveChangesAsync();

            // Remove attributions for user 2 from this family
            await _sut.RemoveMemberAttributionsAsync(family.Id, ownerId: 2);

            Assert.True(_wrapper.Context.ExpenseFamilyAttributions.Any(a => a.ExpenseId == expenseU1.Id));
            Assert.False(_wrapper.Context.ExpenseFamilyAttributions.Any(a => a.ExpenseId == expenseU2.Id));
        }

        // ── CountHeadsAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task CountHeadsAsync_ReturnsCorrectHeadCount()
        {
            var family = await SeedFamilyAsync();
            await SeedMembershipAsync(family.Id, userId: 1, roleId: 1); // Head
            await SeedMembershipAsync(family.Id, userId: 2, roleId: 1); // Head
            await SeedMembershipAsync(family.Id, userId: 3, roleId: 2); // Member

            var count = await _sut.CountHeadsAsync(family.Id, headRoleId: 1);

            Assert.Equal(2, count);
        }

        // ── ExistsWithNameForUserAsync ────────────────────────────────────────

        [Fact]
        public async Task ExistsWithNameForUserAsync_ReturnsTrue_WhenUserIsMemberOfFamilyWithName()
        {
            var family = await SeedFamilyAsync(createdById: 1);
            await SeedMembershipAsync(family.Id, userId: 1, roleId: 1);

            var result = await _sut.ExistsWithNameForUserAsync(family.Name, userId: 1);

            Assert.True(result);
        }

        [Fact]
        public async Task ExistsWithNameForUserAsync_ReturnsFalse_WhenUserNotMember()
        {
            var family = await SeedFamilyAsync(createdById: 1);
            await SeedMembershipAsync(family.Id, userId: 1, roleId: 1);

            var result = await _sut.ExistsWithNameForUserAsync(family.Name, userId: 999);

            Assert.False(result);
        }

        [Fact]
        public async Task ExistsWithNameForUserAsync_ReturnsFalse_WhenExcludeIdMatches()
        {
            var family = await SeedFamilyAsync(createdById: 1);
            await SeedMembershipAsync(family.Id, userId: 1, roleId: 1);

            var result = await _sut.ExistsWithNameForUserAsync(family.Name, userId: 1, excludeId: family.Id);

            Assert.False(result);
        }

        [Fact]
        public async Task ExistsWithNameForUserAsync_ReturnsFalse_WhenFamilyDeleted()
        {
            var family = await SeedFamilyAsync(createdById: 1);
            await SeedMembershipAsync(family.Id, userId: 1, roleId: 1);
            family.IsDeleted = true;
            _wrapper.Context.Families.Update(family);
            await _wrapper.Context.SaveChangesAsync();

            var result = await _sut.ExistsWithNameForUserAsync(family.Name, userId: 1);

            Assert.False(result);
        }

        // ── CountMemberAttributionsAsync ──────────────────────────────────────

        [Fact]
        public async Task CountMemberAttributionsAsync_ReturnsCountForUserInFamily()
        {
            await SeedUserAsync(1);
            await SeedUserAsync(2, "u2@test.com");
            var family = await SeedFamilyAsync(createdById: 1);

            _wrapper.Context.Currencies.Add(new Models.Currency { Id = 3001, Code = "CA1", Name = "Count A1", Symbol = "C", Decimals = 2 });
            await _wrapper.Context.SaveChangesAsync();

            var expense1 = new Models.Expense
            {
                UserId = 1, Amount = 10m, CurrencyId = 3001,
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                CreatedAt = DateTime.UtcNow, CreatedById = 1, CreatedFromId = 1
            };
            var expense2 = new Models.Expense
            {
                UserId = 2, Amount = 20m, CurrencyId = 3001,
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                CreatedAt = DateTime.UtcNow, CreatedById = 2, CreatedFromId = 1
            };
            _wrapper.Context.Expenses.AddRange(expense1, expense2);
            await _wrapper.Context.SaveChangesAsync();

            _wrapper.Context.ExpenseFamilyAttributions.AddRange(
                new Models.ExpenseFamilyAttribution { ExpenseId = expense1.Id, FamilyId = family.Id, AttributedAt = DateTime.UtcNow, AttributedById = 1 },
                new Models.ExpenseFamilyAttribution { ExpenseId = expense2.Id, FamilyId = family.Id, AttributedAt = DateTime.UtcNow, AttributedById = 2 }
            );
            await _wrapper.Context.SaveChangesAsync();

            var count = await _sut.CountMemberAttributionsAsync(family.Id, userId: 1);

            Assert.Equal(1, count);
        }
    }
}
