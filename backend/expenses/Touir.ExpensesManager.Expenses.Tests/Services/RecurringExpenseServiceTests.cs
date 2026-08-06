using Moq;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Models.Lookups;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;
using Touir.ExpensesManager.Expenses.Services;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Tests.Services
{
    public class RecurringExpenseServiceTests
    {
        private static RecurringExpenseService CreateService(
            IRecurringExpenseRepository? repo = null,
            ILookupCacheService? lookupCache = null)
        {
            ILookupCacheService cache;
            if (lookupCache is not null)
            {
                cache = lookupCache;
            }
            else
            {
                var mockCache = new Mock<ILookupCacheService>();
                mockCache.Setup(l => l.GetNameAsync<RecurrenceFrequency>(It.IsAny<int>())).ReturnsAsync("Monthly");
                cache = mockCache.Object;
            }
            return new RecurringExpenseService(repo ?? Mock.Of<IRecurringExpenseRepository>(), cache);
        }

        private static RecurringExpense MakeRecurring(int id = 1, int frequencyId = 2) => new()
        {
            Id = id,
            UserId = 42,
            Description = "Netflix",
            Amount = 15.99m,
            CurrencyId = 1,
            CategoryId = 1,
            FrequencyId = frequencyId,
            NextDueDate = new DateOnly(2026, 9, 1),
            IsActive = true,
            Currency = new Currency { Id = 1, Code = "USD", Name = "US Dollar", Symbol = "$", Decimals = 2 },
            Category = new Category { Id = 1, Name = "Subscriptions" }
        };

        [Fact]
        public async Task GetUpcomingAsync_MapsRepositoryResultsToDto()
        {
            var repo = new Mock<IRecurringExpenseRepository>();
            repo.Setup(r => r.GetUpcomingAsync(42, 5)).ReturnsAsync([MakeRecurring()]);

            var result = (await CreateService(repo.Object).GetUpcomingAsync(42, 5)).ToList();

            Assert.Single(result);
            Assert.Equal("Netflix", result[0].Description);
            Assert.Equal(15.99m, result[0].Amount);
            Assert.Equal("USD", result[0].Currency!.Code);
            Assert.Equal("Subscriptions", result[0].Category!.Name);
            Assert.Equal("Monthly", result[0].Frequency);
            Assert.Equal(new DateOnly(2026, 9, 1), result[0].NextDueDate);
        }

        [Fact]
        public async Task GetUpcomingAsync_ReturnsEmpty_WhenRepositoryReturnsNone()
        {
            var repo = new Mock<IRecurringExpenseRepository>();
            repo.Setup(r => r.GetUpcomingAsync(42, 5)).ReturnsAsync([]);

            var result = await CreateService(repo.Object).GetUpcomingAsync(42, 5);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUpcomingAsync_PassesUserIdAndTakeToRepository()
        {
            var repo = new Mock<IRecurringExpenseRepository>();
            repo.Setup(r => r.GetUpcomingAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync([]);

            await CreateService(repo.Object).GetUpcomingAsync(7, 3);

            repo.Verify(r => r.GetUpcomingAsync(7, 3), Times.Once);
        }

        [Fact]
        public async Task GetUpcomingAsync_HandlesNullCurrencyAndCategoryGracefully()
        {
            var recurring = MakeRecurring();
            recurring.Currency = null;
            recurring.Category = null;
            var repo = new Mock<IRecurringExpenseRepository>();
            repo.Setup(r => r.GetUpcomingAsync(42, 5)).ReturnsAsync([recurring]);

            var result = (await CreateService(repo.Object).GetUpcomingAsync(42, 5)).ToList();

            Assert.Single(result);
            Assert.Null(result[0].Currency);
            Assert.Null(result[0].Category);
        }
    }
}
