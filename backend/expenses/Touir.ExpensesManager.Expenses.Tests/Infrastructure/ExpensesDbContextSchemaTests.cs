using Microsoft.EntityFrameworkCore;
using Touir.ExpensesManager.Expenses.Infrastructure;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Models.External;
using Touir.ExpensesManager.Expenses.Tests.TestHelpers;

namespace Touir.ExpensesManager.Expenses.Tests.Infrastructure
{
    public class ExpensesDbContextSchemaTests : IDisposable
    {
        private readonly TestExpensesDbContextWrapper _wrapper;
        private readonly ExpensesDbContext _ctx;

        public ExpensesDbContextSchemaTests()
        {
            _wrapper = new TestExpensesDbContextWrapper();
            _ctx = _wrapper.Context;
        }

        public void Dispose()
        {
            _wrapper.Dispose();
            GC.SuppressFinalize(this);
        }

        // ── helpers ───────────────────────────────────────────────────────────

        private async Task<User> SeedUserAsync(int id = 1)
        {
            var user = new User { Id = id, FirstName = "Test", LastName = "User", Email = $"u{id}@test.com", IsDeleted = false };
            _ctx.Users.Add(user);
            await _ctx.SaveChangesAsync();
            return user;
        }

        private async Task<Currency> SeedCurrencyAsync(int id = 1)
        {
            var currency = new Currency { Id = id, Code = $"C{id:00}", Name = $"Currency {id}", Symbol = "$", Decimals = 2 };
            _ctx.Currencies.Add(currency);
            await _ctx.SaveChangesAsync();
            return currency;
        }

        private async Task<Category> SeedCategoryAsync(int id = 1, int? parentId = null)
        {
            var category = new Category { Id = id, Name = $"Category {id}", IsArchived = false, ParentCategoryId = parentId };
            _ctx.Categories.Add(category);
            await _ctx.SaveChangesAsync();
            return category;
        }

        private async Task<Family> SeedFamilyAsync(int createdById, int id = 1)
        {
            var family = new Family { Id = id, Name = "Test Family", IsDefault = true, IsArchived = false, CreatedAt = DateTime.UtcNow, CreatedById = createdById };
            _ctx.Families.Add(family);
            await _ctx.SaveChangesAsync();
            return family;
        }

        private async Task<Expense> SeedExpenseAsync(int userId, int currencyId, int createdById, int id = 1, int? categoryId = null)
        {
            var expense = new Expense
            {
                Id = id,
                UserId = userId,
                Amount = 99.99m,
                CurrencyId = currencyId,
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                CategoryId = categoryId,
                Description = "Test expense",
                CreatedAt = DateTime.UtcNow,
                CreatedById = createdById,
                CreatedFromId = 1 // OperationSource: SingleWeb
            };
            _ctx.Expenses.Add(expense);
            await _ctx.SaveChangesAsync();
            return expense;
        }

        private async Task<Tag> SeedTagAsync(int userId, int id = 1)
        {
            var tag = new Tag { Id = id, Name = $"tag-{id}", UserId = userId };
            _ctx.Tags.Add(tag);
            await _ctx.SaveChangesAsync();
            return tag;
        }

        private async Task<ExpenseAuditLog> SeedAuditLogAsync(long expenseId, int performedById, int id = 1)
        {
            var log = new ExpenseAuditLog { Id = id, ExpenseId = expenseId, OperationId = 1, PerformedAt = DateTime.UtcNow, PerformedById = performedById, PerformedFromId = 1 };
            _ctx.ExpenseAuditLogs.Add(log);
            await _ctx.SaveChangesAsync();
            return log;
        }

        // ── Category ──────────────────────────────────────────────────────────

        [Fact]
        public async Task Category_CanPersist_WithIsArchived()
        {
            var category = new Category { Id = 1, Name = "Food", IsArchived = true };
            _ctx.Categories.Add(category);
            await _ctx.SaveChangesAsync();

            _ctx.ChangeTracker.Clear();
            var saved = await _ctx.Categories.FindAsync(1);
            Assert.NotNull(saved);
            Assert.True(saved.IsArchived);
        }

        [Fact]
        public async Task Category_CanPersist_ParentChildHierarchy()
        {
            var parent = new Category { Id = 1, Name = "Food", IsArchived = false };
            var child = new Category { Id = 2, Name = "Groceries", IsArchived = false, ParentCategoryId = 1 };
            _ctx.Categories.AddRange(parent, child);
            await _ctx.SaveChangesAsync();

            _ctx.ChangeTracker.Clear();
            var saved = await _ctx.Categories.Include(c => c.Children).FirstAsync(c => c.Id == 1);
            Assert.Single(saved.Children);
            Assert.Equal(2, saved.Children.First().Id);
        }

        // ── Expense ───────────────────────────────────────────────────────────

        [Fact]
        public async Task Expense_CanPersist_WithRequiredFields()
        {
            var user = await SeedUserAsync();
            var currency = await SeedCurrencyAsync();

            var expense = new Expense
            {
                Id = 1,
                UserId = user.Id,
                Amount = 123.45m,
                CurrencyId = currency.Id,
                Date = new DateOnly(2026, 5, 1),
                Description = "Groceries",
                CreatedAt = DateTime.UtcNow,
                CreatedById = user.Id,
                CreatedFromId = 1 // OperationSource: SingleWeb
            };
            _ctx.Expenses.Add(expense);
            await _ctx.SaveChangesAsync();

            _ctx.ChangeTracker.Clear();
            var saved = await _ctx.Expenses.FindAsync(1L);
            Assert.NotNull(saved);
            Assert.Equal(123.45m, saved.Amount);
            Assert.Equal(new DateOnly(2026, 5, 1), saved.Date);
        }

        [Fact]
        public async Task Expense_CanPersist_WithModifiedFields()
        {
            var user = await SeedUserAsync();
            var user2 = await SeedUserAsync(id: 2);
            var currency = await SeedCurrencyAsync();

            var expense = new Expense
            {
                Id = 1,
                UserId = user.Id,
                Amount = 50m,
                CurrencyId = currency.Id,
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                CreatedAt = DateTime.UtcNow,
                CreatedById = user.Id,
                CreatedFromId = 1, // OperationSource: SingleWeb
                ModifiedAt = DateTime.UtcNow,
                ModifiedById = user2.Id,
                ModifiedFromId = 1 // ModifiedSource: Web
            };
            _ctx.Expenses.Add(expense);
            await _ctx.SaveChangesAsync();

            _ctx.ChangeTracker.Clear();
            var saved = await _ctx.Expenses.FindAsync(1L);
            Assert.NotNull(saved);
            Assert.Equal(user2.Id, saved.ModifiedById);
            Assert.Equal(1, saved.ModifiedFromId);
        }

        [Fact]
        public async Task Expense_CreatedFrom_RoundTripsCorrectly()
        {
            var user = await SeedUserAsync();
            var currency = await SeedCurrencyAsync();

            var expense = new Expense { Id = 1, UserId = user.Id, Amount = 1m, CurrencyId = currency.Id, Date = DateOnly.FromDateTime(DateTime.UtcNow), CreatedAt = DateTime.UtcNow, CreatedById = user.Id, CreatedFromId = 3 /* OperationSource: BulkWeb */ };
            _ctx.Expenses.Add(expense);
            await _ctx.SaveChangesAsync();

            _ctx.ChangeTracker.Clear();
            var saved = await _ctx.Expenses.FindAsync(1L);
            Assert.Equal(3, saved!.CreatedFromId);
        }

        [Fact]
        public async Task Expense_CanPersist_WithSubcategory()
        {
            var user = await SeedUserAsync();
            var currency = await SeedCurrencyAsync();
            var parent = await SeedCategoryAsync(id: 1);
            var sub = await SeedCategoryAsync(id: 2, parentId: 1);

            var expense = new Expense { Id = 1, UserId = user.Id, Amount = 10m, CurrencyId = currency.Id, Date = DateOnly.FromDateTime(DateTime.UtcNow), CategoryId = parent.Id, SubcategoryId = sub.Id, CreatedAt = DateTime.UtcNow, CreatedById = user.Id, CreatedFromId = 1 };
            _ctx.Expenses.Add(expense);
            await _ctx.SaveChangesAsync();

            _ctx.ChangeTracker.Clear();
            var saved = await _ctx.Expenses.FindAsync(1L);
            Assert.Equal(parent.Id, saved!.CategoryId);
            Assert.Equal(sub.Id, saved.SubcategoryId);
        }

        // ── Family ────────────────────────────────────────────────────────────

        [Fact]
        public async Task Family_CanPersist_WithDefaultFlag()
        {
            var user = await SeedUserAsync();
            var family = new Family { Id = 1, Name = "My Family", IsDefault = true, IsArchived = false, CreatedAt = DateTime.UtcNow, CreatedById = user.Id };
            _ctx.Families.Add(family);
            await _ctx.SaveChangesAsync();

            _ctx.ChangeTracker.Clear();
            var saved = await _ctx.Families.FindAsync(1);
            Assert.NotNull(saved);
            Assert.True(saved.IsDefault);
            Assert.False(saved.IsArchived);
        }

        // ── FamilyMembership ──────────────────────────────────────────────────

        [Fact]
        public async Task FamilyMembership_CanPersist_WithRoleHead()
        {
            var user = await SeedUserAsync();
            var family = await SeedFamilyAsync(user.Id);
            var membership = new FamilyMembership { Id = 1, FamilyId = family.Id, UserId = user.Id, RoleId = 1 /* FamilyRole: Head */, JoinedAt = DateTime.UtcNow };
            _ctx.FamilyMemberships.Add(membership);
            await _ctx.SaveChangesAsync();

            _ctx.ChangeTracker.Clear();
            var saved = await _ctx.FamilyMemberships.FindAsync(1);
            Assert.NotNull(saved);
            Assert.Equal(1, saved.RoleId);
        }

        [Fact]
        public async Task FamilyMembership_Role_RoundTripsCorrectly()
        {
            var user = await SeedUserAsync();
            var family = await SeedFamilyAsync(user.Id);
            _ctx.FamilyMemberships.Add(new FamilyMembership { Id = 1, FamilyId = family.Id, UserId = user.Id, RoleId = 2 /* FamilyRole: Member */, JoinedAt = DateTime.UtcNow });
            await _ctx.SaveChangesAsync();

            _ctx.ChangeTracker.Clear();
            var saved = await _ctx.FamilyMemberships.FindAsync(1);
            Assert.Equal(2, saved!.RoleId);
        }

        // ── ExpenseFamilyAttribution ──────────────────────────────────────────

        [Fact]
        public async Task ExpenseFamilyAttribution_CanPersist()
        {
            var user = await SeedUserAsync();
            var currency = await SeedCurrencyAsync();
            var family = await SeedFamilyAsync(user.Id);
            var expense = await SeedExpenseAsync(user.Id, currency.Id, user.Id);

            var attribution = new ExpenseFamilyAttribution { Id = 1, ExpenseId = expense.Id, FamilyId = family.Id, AttributedAt = DateTime.UtcNow, AttributedById = user.Id };
            _ctx.ExpenseFamilyAttributions.Add(attribution);
            await _ctx.SaveChangesAsync();

            _ctx.ChangeTracker.Clear();
            var saved = await _ctx.ExpenseFamilyAttributions.FindAsync(1L);
            Assert.NotNull(saved);
            Assert.Equal(expense.Id, saved.ExpenseId);
            Assert.Equal(family.Id, saved.FamilyId);
        }

        // ── Tag ───────────────────────────────────────────────────────────────

        [Fact]
        public async Task Tag_CanPersist()
        {
            var user = await SeedUserAsync();
            var tag = new Tag { Id = 1, Name = "food", UserId = user.Id };
            _ctx.Tags.Add(tag);
            await _ctx.SaveChangesAsync();

            _ctx.ChangeTracker.Clear();
            var saved = await _ctx.Tags.FindAsync(1);
            Assert.NotNull(saved);
            Assert.Equal("food", saved.Name);
        }

        // ── ExpenseTag ────────────────────────────────────────────────────────

        [Fact]
        public async Task ExpenseTag_CompositePK_CanPersist()
        {
            var user = await SeedUserAsync();
            var currency = await SeedCurrencyAsync();
            var expense = await SeedExpenseAsync(user.Id, currency.Id, user.Id);
            var tag = await SeedTagAsync(user.Id);

            _ctx.ExpenseTags.Add(new ExpenseTag { ExpenseId = expense.Id, TagId = tag.Id });
            await _ctx.SaveChangesAsync();

            _ctx.ChangeTracker.Clear();
            var saved = await _ctx.ExpenseTags.FindAsync(expense.Id, tag.Id);
            Assert.NotNull(saved);
        }

        [Fact]
        public async Task ExpenseTag_DuplicateCompositePK_ThrowsOnSave()
        {
            var user = await SeedUserAsync();
            var currency = await SeedCurrencyAsync();
            var expense = await SeedExpenseAsync(user.Id, currency.Id, user.Id);
            var tag = await SeedTagAsync(user.Id);

            _ctx.ExpenseTags.Add(new ExpenseTag { ExpenseId = expense.Id, TagId = tag.Id });
            await _ctx.SaveChangesAsync();
            _ctx.ChangeTracker.Clear();

            _ctx.ExpenseTags.Add(new ExpenseTag { ExpenseId = expense.Id, TagId = tag.Id });
            await Assert.ThrowsAsync<DbUpdateException>(() => _ctx.SaveChangesAsync());
        }

        // ── CurrencyDailyRate ─────────────────────────────────────────────────

        [Fact]
        public async Task CurrencyDailyRate_CanPersist()
        {
            var src = await SeedCurrencyAsync(id: 1);
            var dst = await SeedCurrencyAsync(id: 2);
            var rate = new CurrencyDailyRate { Id = 1, SourceCurrencyId = src.Id, DestinationCurrencyId = dst.Id, Date = new DateOnly(2026, 5, 1), Rate = 1.12345678m, RateSourceId = 2 /* RateSource: Manual */ };
            _ctx.CurrencyDailyRates.Add(rate);
            await _ctx.SaveChangesAsync();

            _ctx.ChangeTracker.Clear();
            var saved = await _ctx.CurrencyDailyRates.FindAsync(1);
            Assert.NotNull(saved);
            Assert.Equal(2, saved.RateSourceId);
        }

        [Fact]
        public async Task CurrencyDailyRate_UniqueConstraint_ThrowsOnDuplicate()
        {
            var src = await SeedCurrencyAsync(id: 1);
            var dst = await SeedCurrencyAsync(id: 2);
            var date = new DateOnly(2026, 5, 1);

            _ctx.CurrencyDailyRates.Add(new CurrencyDailyRate { Id = 1, SourceCurrencyId = src.Id, DestinationCurrencyId = dst.Id, Date = date, Rate = 1.1m, RateSourceId = 1 /* RateSource: Auto */ });
            await _ctx.SaveChangesAsync();
            _ctx.ChangeTracker.Clear();

            _ctx.CurrencyDailyRates.Add(new CurrencyDailyRate { Id = 2, SourceCurrencyId = src.Id, DestinationCurrencyId = dst.Id, Date = date, Rate = 1.2m, RateSourceId = 2 /* RateSource: Manual */ });
            await Assert.ThrowsAsync<DbUpdateException>(() => _ctx.SaveChangesAsync());
        }

        // ── CurrencyPairDefault ───────────────────────────────────────────────

        [Fact]
        public async Task CurrencyPairDefault_CompositePK_CanPersist()
        {
            var src = await SeedCurrencyAsync(id: 1);
            var dst = await SeedCurrencyAsync(id: 2);
            var pair = new CurrencyPairDefault { SourceCurrencyId = src.Id, DestinationCurrencyId = dst.Id, Rate = 1.5m };
            _ctx.CurrencyPairDefaults.Add(pair);
            await _ctx.SaveChangesAsync();

            _ctx.ChangeTracker.Clear();
            var saved = await _ctx.CurrencyPairDefaults.FindAsync(src.Id, dst.Id);
            Assert.NotNull(saved);
            Assert.Equal(1.5m, saved.Rate);
        }

        [Fact]
        public async Task CurrencyPairDefault_DuplicateCompositePK_ThrowsOnSave()
        {
            var src = await SeedCurrencyAsync(id: 1);
            var dst = await SeedCurrencyAsync(id: 2);

            _ctx.CurrencyPairDefaults.Add(new CurrencyPairDefault { SourceCurrencyId = src.Id, DestinationCurrencyId = dst.Id, Rate = 1.5m });
            await _ctx.SaveChangesAsync();
            _ctx.ChangeTracker.Clear();

            _ctx.CurrencyPairDefaults.Add(new CurrencyPairDefault { SourceCurrencyId = src.Id, DestinationCurrencyId = dst.Id, Rate = 2.0m });
            await Assert.ThrowsAsync<DbUpdateException>(() => _ctx.SaveChangesAsync());
        }

        // ── CurrencyRateConflict ──────────────────────────────────────────────

        [Fact]
        public async Task CurrencyRateConflict_CanPersist_Pending()
        {
            var src = await SeedCurrencyAsync(id: 1);
            var dst = await SeedCurrencyAsync(id: 2);
            var conflict = new CurrencyRateConflict { Id = 1, SourceCurrencyId = src.Id, DestinationCurrencyId = dst.Id, Date = new DateOnly(2026, 5, 1), AutomaticRate = 1.1m, ManualRate = 1.2m, StatusId = 1 /* ConflictStatus: Pending */ };
            _ctx.CurrencyRateConflicts.Add(conflict);
            await _ctx.SaveChangesAsync();

            _ctx.ChangeTracker.Clear();
            var saved = await _ctx.CurrencyRateConflicts.FindAsync(1);
            Assert.NotNull(saved);
            Assert.Equal(1, saved.StatusId);
            Assert.Null(saved.ResolutionId);
            Assert.Null(saved.ResolvedAt);
            Assert.Null(saved.ResolvedById);
        }

        [Fact]
        public async Task CurrencyRateConflict_CanPersist_Resolved_WithCustomRate()
        {
            var user = await SeedUserAsync();
            var src = await SeedCurrencyAsync(id: 1);
            var dst = await SeedCurrencyAsync(id: 2);
            var conflict = new CurrencyRateConflict { Id = 1, SourceCurrencyId = src.Id, DestinationCurrencyId = dst.Id, Date = new DateOnly(2026, 5, 1), AutomaticRate = 1.1m, ManualRate = 1.2m, StatusId = 2 /* ConflictStatus: Resolved */, ResolvedAt = DateTime.UtcNow, ResolvedById = user.Id, ResolutionId = 3 /* ConflictResolution: Custom */, CustomRate = 1.15m };
            _ctx.CurrencyRateConflicts.Add(conflict);
            await _ctx.SaveChangesAsync();

            _ctx.ChangeTracker.Clear();
            var saved = await _ctx.CurrencyRateConflicts.FindAsync(1);
            Assert.Equal(3, saved!.ResolutionId);
            Assert.Equal(1.15m, saved.CustomRate);
            Assert.Equal(user.Id, saved.ResolvedById);
        }

        // ── ExpenseAuditLog ───────────────────────────────────────────────────

        [Fact]
        public async Task ExpenseAuditLog_CanPersist_AllOperations()
        {
            var user = await SeedUserAsync();
            var currency = await SeedCurrencyAsync();
            var expense = await SeedExpenseAsync(user.Id, currency.Id, user.Id);

            // AuditOperation: 1=Add, 2=Update, 3=Delete
            var operationIds = new[] { 1, 2, 3 };
            for (int i = 0; i < operationIds.Length; i++)
            {
                _ctx.ExpenseAuditLogs.Add(new ExpenseAuditLog { Id = i + 1, ExpenseId = expense.Id, OperationId = operationIds[i], PerformedAt = DateTime.UtcNow, PerformedById = user.Id, PerformedFromId = 1 });
            }
            await _ctx.SaveChangesAsync();

            _ctx.ChangeTracker.Clear();
            var logs = await _ctx.ExpenseAuditLogs.ToListAsync();
            Assert.Equal(3, logs.Count);
            Assert.Contains(logs, l => l.OperationId == 1);
            Assert.Contains(logs, l => l.OperationId == 2);
            Assert.Contains(logs, l => l.OperationId == 3);
        }

        // ── ExpenseAuditSnapshot ──────────────────────────────────────────────

        [Fact]
        public async Task ExpenseAuditSnapshot_CanPersist_BeforeAndAfter()
        {
            var user = await SeedUserAsync();
            var currency = await SeedCurrencyAsync();
            var expense = await SeedExpenseAsync(user.Id, currency.Id, user.Id);
            var log = await SeedAuditLogAsync(expense.Id, user.Id);

            // SnapshotType: 1=Before, 2=After
            var snapshots = new[]
            {
                new ExpenseAuditSnapshot { Id = 1, AuditLogId = log.Id, SnapshotTypeId = 1, Amount = 50m, CurrencyId = currency.Id, Date = DateOnly.FromDateTime(DateTime.UtcNow), Description = "old desc", Tags = "1,2", Families = "1" },
                new ExpenseAuditSnapshot { Id = 2, AuditLogId = log.Id, SnapshotTypeId = 2, Amount = 75m, CurrencyId = currency.Id, Date = DateOnly.FromDateTime(DateTime.UtcNow), Description = "new desc", Tags = "1,2,3", Families = "1" }
            };
            _ctx.ExpenseAuditSnapshots.AddRange(snapshots);
            await _ctx.SaveChangesAsync();

            _ctx.ChangeTracker.Clear();
            var saved = await _ctx.ExpenseAuditSnapshots.Where(s => s.AuditLogId == log.Id).ToListAsync();
            Assert.Equal(2, saved.Count);
            Assert.Contains(saved, s => s.SnapshotTypeId == 1 && s.Amount == 50m);
            Assert.Contains(saved, s => s.SnapshotTypeId == 2 && s.Amount == 75m);
        }

        // ── Cascade delete ────────────────────────────────────────────────────

        [Fact]
        public async Task ExpenseAuditLog_CascadeDelete_WhenExpenseDeleted()
        {
            var user = await SeedUserAsync();
            var currency = await SeedCurrencyAsync();
            var expense = await SeedExpenseAsync(user.Id, currency.Id, user.Id);
            await SeedAuditLogAsync(expense.Id, user.Id);

            _ctx.Expenses.Remove(expense);
            await _ctx.SaveChangesAsync();

            _ctx.ChangeTracker.Clear();
            var remaining = await _ctx.ExpenseAuditLogs.CountAsync();
            Assert.Equal(0, remaining);
        }

        [Fact]
        public async Task FamilyMembership_CascadeDelete_WhenFamilyDeleted()
        {
            var user = await SeedUserAsync();
            var family = await SeedFamilyAsync(user.Id);
            _ctx.FamilyMemberships.Add(new FamilyMembership { Id = 1, FamilyId = family.Id, UserId = user.Id, RoleId = 1 /* FamilyRole: Head */, JoinedAt = DateTime.UtcNow });
            await _ctx.SaveChangesAsync();

            _ctx.Families.Remove(family);
            await _ctx.SaveChangesAsync();

            _ctx.ChangeTracker.Clear();
            var remaining = await _ctx.FamilyMemberships.CountAsync();
            Assert.Equal(0, remaining);
        }
    }
}
