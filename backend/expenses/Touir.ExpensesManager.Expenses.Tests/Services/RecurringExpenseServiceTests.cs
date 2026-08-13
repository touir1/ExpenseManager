using Moq;
using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Requests;
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
            ILookupCacheService? lookupCache = null,
            IExpenseService? expenseService = null)
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
            return new RecurringExpenseService(
                repo ?? Mock.Of<IRecurringExpenseRepository>(),
                cache,
                expenseService ?? Mock.Of<IExpenseService>());
        }

        private static RecurringExpense MakeRecurring(int id = 1, int frequencyId = 2, int userId = 42, bool autoCreate = false) => new()
        {
            Id = id,
            UserId = userId,
            Description = "Netflix",
            Amount = 15.99m,
            CurrencyId = 1,
            CategoryId = 1,
            FrequencyId = frequencyId,
            NextDueDate = new DateOnly(2026, 9, 1),
            IsActive = true,
            AutoCreate = autoCreate,
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

        // ── CreateAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_AddsAndReturnsDto()
        {
            var repo = new Mock<IRecurringExpenseRepository>();
            RecurringExpense? added = null;
            repo.Setup(r => r.AddAsync(It.IsAny<RecurringExpense>()))
                .Callback<RecurringExpense>(r => { r.Id = 5; added = r; })
                .ReturnsAsync(() => added!);
            repo.Setup(r => r.GetByIdAsync(5, 42)).ReturnsAsync(() => added);

            var request = new CreateRecurringExpenseRequest
            {
                Description = "Netflix",
                Amount = 15.99m,
                CurrencyId = 1,
                CategoryId = 1,
                FrequencyId = 2,
                NextDueDate = new DateOnly(2026, 9, 1),
                AutoCreate = true
            };

            var dto = await CreateService(repo.Object).CreateAsync(request, 42);

            Assert.Equal(5, dto.Id);
            Assert.Equal("Netflix", dto.Description);
            Assert.True(dto.AutoCreate);
            Assert.True(dto.IsActive);
            repo.Verify(r => r.AddAsync(It.Is<RecurringExpense>(x => x.UserId == 42)), Times.Once);
        }

        // ── UpdateAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_ReturnsNull_WhenNotFoundOrNotOwned()
        {
            var repo = new Mock<IRecurringExpenseRepository>();
            repo.Setup(r => r.GetByIdAsync(1, 42)).ReturnsAsync((RecurringExpense?)null);

            var request = new UpdateRecurringExpenseRequest
            {
                Description = "X",
                Amount = 1,
                CurrencyId = 1,
                CategoryId = 1,
                FrequencyId = 1,
                NextDueDate = new DateOnly(2026, 9, 1)
            };

            var result = await CreateService(repo.Object).UpdateAsync(1, request, 42);

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesFieldsAndReturnsDto()
        {
            var existing = MakeRecurring();
            var repo = new Mock<IRecurringExpenseRepository>();
            repo.SetupSequence(r => r.GetByIdAsync(1, 42))
                .ReturnsAsync(existing)
                .ReturnsAsync(existing);

            var request = new UpdateRecurringExpenseRequest
            {
                Description = "Updated",
                Amount = 20m,
                CurrencyId = 1,
                CategoryId = 1,
                FrequencyId = 3,
                NextDueDate = new DateOnly(2026, 10, 1),
                AutoCreate = true,
                IsActive = false
            };

            var dto = await CreateService(repo.Object).UpdateAsync(1, request, 42);

            Assert.NotNull(dto);
            repo.Verify(r => r.UpdateAsync(It.Is<RecurringExpense>(x =>
                x.Description == "Updated" && x.Amount == 20m && x.FrequencyId == 3 && x.AutoCreate && !x.IsActive)), Times.Once);
        }

        // ── DeleteAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteAsync_DelegatesToRepository()
        {
            var repo = new Mock<IRecurringExpenseRepository>();
            repo.Setup(r => r.SoftDeleteAsync(1, 42)).ReturnsAsync(true);

            var result = await CreateService(repo.Object).DeleteAsync(1, 42);

            Assert.True(result);
            repo.Verify(r => r.SoftDeleteAsync(1, 42), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
        {
            var repo = new Mock<IRecurringExpenseRepository>();
            repo.Setup(r => r.SoftDeleteAsync(1, 42)).ReturnsAsync(false);

            var result = await CreateService(repo.Object).DeleteAsync(1, 42);

            Assert.False(result);
        }

        // ── ConfirmAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task ConfirmAsync_ReturnsNull_WhenNotFoundOrNotOwned()
        {
            var repo = new Mock<IRecurringExpenseRepository>();
            repo.Setup(r => r.GetByIdAsync(1, 42)).ReturnsAsync((RecurringExpense?)null);

            var result = await CreateService(repo.Object).ConfirmAsync(1, 42);

            Assert.Null(result);
        }

        [Fact]
        public async Task ConfirmAsync_Throws_WhenNotYetDue()
        {
            var existing = MakeRecurring();
            existing.NextDueDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(5);
            var repo = new Mock<IRecurringExpenseRepository>();
            repo.Setup(r => r.GetByIdAsync(1, 42)).ReturnsAsync(existing);

            await Assert.ThrowsAsync<RecurringExpenseNotDueException>(
                () => CreateService(repo.Object).ConfirmAsync(1, 42));
        }

        [Fact]
        public async Task ConfirmAsync_CreatesExpenseAndAdvancesTemplate_WhenDue()
        {
            var existing = MakeRecurring(frequencyId: 2);
            existing.NextDueDate = DateOnly.FromDateTime(DateTime.UtcNow);
            var dueDate = existing.NextDueDate;
            var repo = new Mock<IRecurringExpenseRepository>();
            repo.Setup(r => r.GetByIdAsync(1, 42)).ReturnsAsync(existing);

            var expenseService = new Mock<IExpenseService>();
            var expenseDto = new ExpenseDto { Id = 100 };
            expenseService.Setup(s => s.AddAsync(It.IsAny<CreateExpenseRequest>(), 42, 1)).ReturnsAsync(expenseDto);

            var result = await CreateService(repo.Object, expenseService: expenseService.Object).ConfirmAsync(1, 42);

            Assert.NotNull(result);
            Assert.Equal(100, result!.Id);
            Assert.Equal(dueDate.AddMonths(1), existing.NextDueDate);
            Assert.Equal(dueDate, existing.LastGeneratedDate);
            repo.Verify(r => r.UpdateAsync(existing), Times.Once);
        }

        // ── AdvanceNextDueDate (via RecurringExpenseScheduler) ──────────────

        [Theory]
        [InlineData(1, "2026-08-10", "2026-08-17")] // Weekly +7d
        [InlineData(2, "2026-08-10", "2026-09-10")] // Monthly
        [InlineData(3, "2026-08-10", "2027-08-10")] // Yearly
        public void AdvanceNextDueDate_ComputesExpectedNextDate(int frequencyId, string current, string expected)
        {
            var result = RecurringExpenseScheduler.AdvanceNextDueDate(DateOnly.Parse(current), frequencyId);

            Assert.Equal(DateOnly.Parse(expected), result);
        }

        [Fact]
        public void AdvanceNextDueDate_Monthly_HandlesMonthEndEdgeCase()
        {
            // Jan 31 + 1 month -> Feb 28 (non-leap year 2026)
            var result = RecurringExpenseScheduler.AdvanceNextDueDate(new DateOnly(2026, 1, 31), 2);

            Assert.Equal(new DateOnly(2026, 2, 28), result);
        }

        [Fact]
        public void AdvanceNextDueDate_Monthly_HandlesLeapYearMonthEnd()
        {
            // Jan 31 + 1 month -> Feb 29 (leap year 2028)
            var result = RecurringExpenseScheduler.AdvanceNextDueDate(new DateOnly(2028, 1, 31), 2);

            Assert.Equal(new DateOnly(2028, 2, 29), result);
        }

        // ── GenerateDueAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task GenerateDueAsync_CreatesExpenses_OnlyForAutoCreateItems()
        {
            var autoItem = MakeRecurring(id: 1, userId: 42, autoCreate: true);
            var manualItem = MakeRecurring(id: 2, userId: 43, autoCreate: false);
            var repo = new Mock<IRecurringExpenseRepository>();
            repo.Setup(r => r.GetDueForGenerationAsync(It.IsAny<DateOnly>())).ReturnsAsync([autoItem, manualItem]);

            var expenseService = new Mock<IExpenseService>();
            expenseService.Setup(s => s.AddAsync(It.IsAny<CreateExpenseRequest>(), It.IsAny<int>(), 3))
                .ReturnsAsync(new ExpenseDto { Id = 1 });

            await CreateService(repo.Object, expenseService: expenseService.Object)
                .GenerateDueAsync(DateOnly.FromDateTime(DateTime.UtcNow));

            expenseService.Verify(s => s.AddAsync(It.IsAny<CreateExpenseRequest>(), autoItem.UserId, 3), Times.Once);
            expenseService.Verify(s => s.AddAsync(It.IsAny<CreateExpenseRequest>(), manualItem.UserId, 3), Times.Never);
            repo.Verify(r => r.UpdateAsync(It.Is<RecurringExpense>(x => x.Id == 1)), Times.Once);
        }

        [Fact]
        public async Task GenerateDueAsync_ContinuesBatch_WhenOneRowThrows()
        {
            var badItem = MakeRecurring(id: 1, autoCreate: true);
            var goodItem = MakeRecurring(id: 2, autoCreate: true);
            var repo = new Mock<IRecurringExpenseRepository>();
            repo.Setup(r => r.GetDueForGenerationAsync(It.IsAny<DateOnly>())).ReturnsAsync([badItem, goodItem]);

            var expenseService = new Mock<IExpenseService>();
            expenseService.SetupSequence(s => s.AddAsync(It.IsAny<CreateExpenseRequest>(), It.IsAny<int>(), 3))
                .ThrowsAsync(new InvalidOperationException("boom"))
                .ReturnsAsync(new ExpenseDto { Id = 2 });

            await CreateService(repo.Object, expenseService: expenseService.Object)
                .GenerateDueAsync(DateOnly.FromDateTime(DateTime.UtcNow));

            expenseService.Verify(s => s.AddAsync(It.IsAny<CreateExpenseRequest>(), It.IsAny<int>(), 3), Times.Exactly(2));
            repo.Verify(r => r.UpdateAsync(It.Is<RecurringExpense>(x => x.Id == 2)), Times.Once);
            repo.Verify(r => r.UpdateAsync(It.Is<RecurringExpense>(x => x.Id == 1)), Times.Never);
        }
    }
}
