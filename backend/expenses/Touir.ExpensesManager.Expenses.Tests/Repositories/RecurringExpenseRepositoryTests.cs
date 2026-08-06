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
            int frequencyId = 2) => new()
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
    }
}
