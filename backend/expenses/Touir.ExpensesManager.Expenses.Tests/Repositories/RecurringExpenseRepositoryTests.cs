using Microsoft.EntityFrameworkCore;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Models.External;
using Touir.ExpensesManager.Expenses.Repositories;
using Touir.ExpensesManager.Expenses.Tests.TestHelpers;

namespace Touir.ExpensesManager.Expenses.Tests.Repositories
{
    public class RecurringExpenseRepositoryTests : IDisposable
    {
        private readonly TestExpensesDbContextWrapper _wrapper;
        private readonly RecurringExpenseRepository _sut;

        public RecurringExpenseRepositoryTests()
        {
            _wrapper = new TestExpensesDbContextWrapper();
            _sut = new RecurringExpenseRepository(_wrapper.Context);
        }

        public void Dispose()
        {
            _wrapper.Dispose();
            GC.SuppressFinalize(this);
        }

        private async Task SeedUserAsync(int id)
        {
            if (!_wrapper.Context.Users.Any(u => u.Id == id))
            {
                _wrapper.Context.Users.Add(new User { Id = id, FirstName = "T", LastName = "U", Email = $"u{id}@test.com", IsDeleted = false });
                await _wrapper.Context.SaveChangesAsync();
            }
        }

        private RecurringExpense MakeRecurring(
            int userId,
            string description,
            DateOnly nextDueDate,
            bool isActive = true,
            bool isDeleted = false,
            int frequencyId = 2,
            bool autoCreate = false,
            DateOnly? lastGeneratedDate = null) => new()
        {
            UserId = userId,
            Description = description,
            Amount = 10m,
            CurrencyId = 1,
            CategoryId = 1,
            FrequencyId = frequencyId,
            NextDueDate = nextDueDate,
            IsActive = isActive,
            IsDeleted = isDeleted,
            AutoCreate = autoCreate,
            LastGeneratedDate = lastGeneratedDate,
            CreatedAt = DateTime.UtcNow
        };

        // ── GetUpcomingAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task GetUpcomingAsync_ReturnsActiveItems_SortedByDueDateAscending()
        {
            await SeedUserAsync(1);
            _wrapper.Context.RecurringExpenses.AddRange(
                MakeRecurring(1, "Later", new DateOnly(2026, 9, 1)),
                MakeRecurring(1, "Sooner", new DateOnly(2026, 8, 10)));
            await _wrapper.Context.SaveChangesAsync();

            var result = (await _sut.GetUpcomingAsync(1, 5)).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal("Sooner", result[0].Description);
            Assert.Equal("Later", result[1].Description);
        }

        [Fact]
        public async Task GetUpcomingAsync_ExcludesInactive()
        {
            await SeedUserAsync(1);
            _wrapper.Context.RecurringExpenses.Add(MakeRecurring(1, "Inactive", new DateOnly(2026, 8, 10), isActive: false));
            await _wrapper.Context.SaveChangesAsync();

            var result = await _sut.GetUpcomingAsync(1, 5);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUpcomingAsync_ExcludesDeleted()
        {
            await SeedUserAsync(1);
            _wrapper.Context.RecurringExpenses.Add(MakeRecurring(1, "Deleted", new DateOnly(2026, 8, 10), isDeleted: true));
            await _wrapper.Context.SaveChangesAsync();

            var result = await _sut.GetUpcomingAsync(1, 5);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUpcomingAsync_ExcludesOtherUsers()
        {
            await SeedUserAsync(1);
            await SeedUserAsync(2);
            _wrapper.Context.RecurringExpenses.Add(MakeRecurring(2, "Other", new DateOnly(2026, 8, 10)));
            await _wrapper.Context.SaveChangesAsync();

            var result = await _sut.GetUpcomingAsync(1, 5);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUpcomingAsync_RespectsTakeLimit()
        {
            await SeedUserAsync(1);
            for (var i = 0; i < 8; i++)
            {
                _wrapper.Context.RecurringExpenses.Add(MakeRecurring(1, $"Item{i}", new DateOnly(2026, 8, 1 + i)));
            }
            await _wrapper.Context.SaveChangesAsync();

            var result = await _sut.GetUpcomingAsync(1, 3);

            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task GetUpcomingAsync_ReturnsEmpty_WhenNoneScheduled()
        {
            await SeedUserAsync(1);

            var result = await _sut.GetUpcomingAsync(1, 5);

            Assert.Empty(result);
        }

        // ── GetPagedAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task GetPagedAsync_ScopedToUser_ExcludesOthers()
        {
            await SeedUserAsync(1);
            await SeedUserAsync(2);
            _wrapper.Context.RecurringExpenses.AddRange(
                MakeRecurring(1, "Mine", new DateOnly(2026, 8, 10)),
                MakeRecurring(2, "Other", new DateOnly(2026, 8, 11)));
            await _wrapper.Context.SaveChangesAsync();

            var result = (await _sut.GetPagedAsync(1, includeInactive: true)).ToList();

            Assert.Single(result);
            Assert.Equal("Mine", result[0].Description);
        }

        [Fact]
        public async Task GetPagedAsync_ExcludesInactive_WhenNotRequested()
        {
            await SeedUserAsync(1);
            _wrapper.Context.RecurringExpenses.AddRange(
                MakeRecurring(1, "Active", new DateOnly(2026, 8, 10), isActive: true),
                MakeRecurring(1, "Paused", new DateOnly(2026, 8, 11), isActive: false));
            await _wrapper.Context.SaveChangesAsync();

            var result = (await _sut.GetPagedAsync(1, includeInactive: false)).ToList();

            Assert.Single(result);
            Assert.Equal("Active", result[0].Description);
        }

        [Fact]
        public async Task GetPagedAsync_IncludesInactive_WhenRequested()
        {
            await SeedUserAsync(1);
            _wrapper.Context.RecurringExpenses.AddRange(
                MakeRecurring(1, "Active", new DateOnly(2026, 8, 10), isActive: true),
                MakeRecurring(1, "Paused", new DateOnly(2026, 8, 11), isActive: false));
            await _wrapper.Context.SaveChangesAsync();

            var result = (await _sut.GetPagedAsync(1, includeInactive: true)).ToList();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetPagedAsync_ExcludesDeleted()
        {
            await SeedUserAsync(1);
            _wrapper.Context.RecurringExpenses.Add(MakeRecurring(1, "Deleted", new DateOnly(2026, 8, 10), isDeleted: true));
            await _wrapper.Context.SaveChangesAsync();

            var result = await _sut.GetPagedAsync(1, includeInactive: true);

            Assert.Empty(result);
        }

        // ── GetByIdAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_ReturnsItem_WhenOwnedByUser()
        {
            await SeedUserAsync(1);
            var entity = MakeRecurring(1, "Mine", new DateOnly(2026, 8, 10));
            _wrapper.Context.RecurringExpenses.Add(entity);
            await _wrapper.Context.SaveChangesAsync();

            var result = await _sut.GetByIdAsync(entity.Id, 1);

            Assert.NotNull(result);
            Assert.Equal("Mine", result!.Description);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotOwnedByUser()
        {
            await SeedUserAsync(1);
            await SeedUserAsync(2);
            var entity = MakeRecurring(2, "Other", new DateOnly(2026, 8, 10));
            _wrapper.Context.RecurringExpenses.Add(entity);
            await _wrapper.Context.SaveChangesAsync();

            var result = await _sut.GetByIdAsync(entity.Id, 1);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenDeleted()
        {
            await SeedUserAsync(1);
            var entity = MakeRecurring(1, "Deleted", new DateOnly(2026, 8, 10), isDeleted: true);
            _wrapper.Context.RecurringExpenses.Add(entity);
            await _wrapper.Context.SaveChangesAsync();

            var result = await _sut.GetByIdAsync(entity.Id, 1);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
        {
            await SeedUserAsync(1);

            var result = await _sut.GetByIdAsync(999, 1);

            Assert.Null(result);
        }

        // ── AddAsync ─────────────────────────────────────────────────────────

        [Fact]
        public async Task AddAsync_PersistsAndAssignsId()
        {
            await SeedUserAsync(1);
            var entity = MakeRecurring(1, "New", new DateOnly(2026, 8, 10));

            var result = await _sut.AddAsync(entity);

            Assert.True(result.Id > 0);
            Assert.Equal(1, await _wrapper.Context.RecurringExpenses.CountAsync());
        }

        // ── UpdateAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_PersistsChanges()
        {
            await SeedUserAsync(1);
            var entity = MakeRecurring(1, "Original", new DateOnly(2026, 8, 10));
            _wrapper.Context.RecurringExpenses.Add(entity);
            await _wrapper.Context.SaveChangesAsync();

            entity.Description = "Changed";
            await _sut.UpdateAsync(entity);

            var reloaded = await _wrapper.Context.RecurringExpenses.FindAsync(entity.Id);
            Assert.Equal("Changed", reloaded!.Description);
        }

        // ── SoftDeleteAsync ──────────────────────────────────────────────────

        [Fact]
        public async Task SoftDeleteAsync_MarksDeleted_WhenOwnedByUser()
        {
            await SeedUserAsync(1);
            var entity = MakeRecurring(1, "ToDelete", new DateOnly(2026, 8, 10));
            _wrapper.Context.RecurringExpenses.Add(entity);
            await _wrapper.Context.SaveChangesAsync();

            var result = await _sut.SoftDeleteAsync(entity.Id, 1);

            Assert.True(result);
            var reloaded = await _wrapper.Context.RecurringExpenses.FindAsync(entity.Id);
            Assert.True(reloaded!.IsDeleted);
            Assert.NotNull(reloaded.DeletedAt);
        }

        [Fact]
        public async Task SoftDeleteAsync_ReturnsFalse_WhenNotOwnedByUser()
        {
            await SeedUserAsync(1);
            await SeedUserAsync(2);
            var entity = MakeRecurring(2, "Other", new DateOnly(2026, 8, 10));
            _wrapper.Context.RecurringExpenses.Add(entity);
            await _wrapper.Context.SaveChangesAsync();

            var result = await _sut.SoftDeleteAsync(entity.Id, 1);

            Assert.False(result);
        }

        [Fact]
        public async Task SoftDeleteAsync_ReturnsFalse_WhenNotFound()
        {
            await SeedUserAsync(1);

            var result = await _sut.SoftDeleteAsync(999, 1);

            Assert.False(result);
        }

        // ── GetDueForGenerationAsync ─────────────────────────────────────────

        [Fact]
        public async Task GetDueForGenerationAsync_IncludesItemsDueToday()
        {
            await SeedUserAsync(1);
            var today = new DateOnly(2026, 8, 13);
            _wrapper.Context.RecurringExpenses.Add(MakeRecurring(1, "Due", today));
            await _wrapper.Context.SaveChangesAsync();

            var result = await _sut.GetDueForGenerationAsync(today);

            Assert.Single(result);
        }

        [Fact]
        public async Task GetDueForGenerationAsync_IncludesOverdueItems()
        {
            await SeedUserAsync(1);
            var today = new DateOnly(2026, 8, 13);
            _wrapper.Context.RecurringExpenses.Add(MakeRecurring(1, "Overdue", today.AddDays(-5)));
            await _wrapper.Context.SaveChangesAsync();

            var result = await _sut.GetDueForGenerationAsync(today);

            Assert.Single(result);
        }

        [Fact]
        public async Task GetDueForGenerationAsync_ExcludesNotYetDue()
        {
            await SeedUserAsync(1);
            var today = new DateOnly(2026, 8, 13);
            _wrapper.Context.RecurringExpenses.Add(MakeRecurring(1, "Future", today.AddDays(5)));
            await _wrapper.Context.SaveChangesAsync();

            var result = await _sut.GetDueForGenerationAsync(today);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetDueForGenerationAsync_ExcludesAlreadyGeneratedToday()
        {
            await SeedUserAsync(1);
            var today = new DateOnly(2026, 8, 13);
            _wrapper.Context.RecurringExpenses.Add(MakeRecurring(1, "AlreadyDone", today, lastGeneratedDate: today));
            await _wrapper.Context.SaveChangesAsync();

            var result = await _sut.GetDueForGenerationAsync(today);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetDueForGenerationAsync_IncludesWhenLastGeneratedBeforeNextDueDate()
        {
            await SeedUserAsync(1);
            var today = new DateOnly(2026, 8, 13);
            _wrapper.Context.RecurringExpenses.Add(MakeRecurring(1, "AdvancedSince", today, lastGeneratedDate: today.AddMonths(-1)));
            await _wrapper.Context.SaveChangesAsync();

            var result = await _sut.GetDueForGenerationAsync(today);

            Assert.Single(result);
        }

        [Fact]
        public async Task GetDueForGenerationAsync_ExcludesInactive()
        {
            await SeedUserAsync(1);
            var today = new DateOnly(2026, 8, 13);
            _wrapper.Context.RecurringExpenses.Add(MakeRecurring(1, "Inactive", today, isActive: false));
            await _wrapper.Context.SaveChangesAsync();

            var result = await _sut.GetDueForGenerationAsync(today);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetDueForGenerationAsync_ExcludesDeleted()
        {
            await SeedUserAsync(1);
            var today = new DateOnly(2026, 8, 13);
            _wrapper.Context.RecurringExpenses.Add(MakeRecurring(1, "Deleted", today, isDeleted: true));
            await _wrapper.Context.SaveChangesAsync();

            var result = await _sut.GetDueForGenerationAsync(today);

            Assert.Empty(result);
        }
    }
}
