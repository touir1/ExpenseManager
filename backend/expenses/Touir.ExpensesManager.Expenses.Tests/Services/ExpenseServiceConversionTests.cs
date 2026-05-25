using Moq;
using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Requests;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;
using Touir.ExpensesManager.Expenses.Services;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Tests.Services
{
    public class ExpenseServiceConversionTests
    {
        private static readonly Currency EurCurrency = new() { Id = 2, Code = "EUR", Name = "Euro", Symbol = "€", Decimals = 2 };
        private static readonly Currency GbpCurrency = new() { Id = 3, Code = "GBP", Name = "Pound", Symbol = "£", Decimals = 2 };

        private static ExpenseService CreateService(
            IExpenseRepository? repo = null,
            ICurrencyRateService? rateService = null,
            ICurrencyRepository? currencyRepo = null)
        {
            return new ExpenseService(
                repo ?? Mock.Of<IExpenseRepository>(),
                Mock.Of<IExpenseAuditService>(),
                Mock.Of<IFamilyRepository>(),
                Mock.Of<ITagRepository>(),
                rateService ?? Mock.Of<ICurrencyRateService>(),
                currencyRepo ?? Mock.Of<ICurrencyRepository>());
        }

        private static Expense MakeExpense(int currencyId = 1) => new()
        {
            Id = 1, UserId = 42, Amount = 100m, CurrencyId = currencyId,
            Date = new DateOnly(2024, 6, 1), CreatedAt = DateTime.UtcNow, CreatedById = 42, CreatedFromId = 1,
            Currency = new Currency { Id = currencyId, Code = "USD", Name = "Dollar", Symbol = "$", Decimals = 2 }
        };

        [Fact]
        public async Task GetByIdAsync_NoDisplayCurrency_ConvertedAmountIsNull()
        {
            var expense = MakeExpense();
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetByIdAsync(1, 42)).ReturnsAsync(expense);

            var result = await CreateService(repo.Object).GetByIdAsync(1, 42, displayCurrencyId: null);

            Assert.Null(result!.ConvertedAmount);
            Assert.Null(result.DisplayCurrency);
        }

        [Fact]
        public async Task GetByIdAsync_SameCurrency_ConvertedAmountIsNull()
        {
            var expense = MakeExpense(currencyId: 2);
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetByIdAsync(1, 42)).ReturnsAsync(expense);
            var rateService = new Mock<ICurrencyRateService>();
            rateService.Setup(r => r.ResolveRateAsync(2, 2, It.IsAny<DateOnly>())).ReturnsAsync(1m);

            var result = await CreateService(repo.Object, rateService.Object).GetByIdAsync(1, 42, displayCurrencyId: 2);

            Assert.Null(result!.ConvertedAmount);
        }

        [Fact]
        public async Task GetByIdAsync_DisplayCurrencySet_PopulatesConvertedAmountAndDisplayCurrency()
        {
            var expense = MakeExpense(currencyId: 1);
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetByIdAsync(1, 42)).ReturnsAsync(expense);
            var rateService = new Mock<ICurrencyRateService>();
            rateService.Setup(r => r.ResolveRateAsync(1, 2, expense.Date)).ReturnsAsync(0.92m);
            var currencyRepo = new Mock<ICurrencyRepository>();
            currencyRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(EurCurrency);

            var result = await CreateService(repo.Object, rateService.Object, currencyRepo.Object).GetByIdAsync(1, 42, displayCurrencyId: 2);

            Assert.Equal(92m, result!.ConvertedAmount); // 100 * 0.92
            Assert.NotNull(result.DisplayCurrency);
            Assert.Equal("EUR", result.DisplayCurrency!.Code);
            Assert.Equal("€", result.DisplayCurrency.Symbol);
        }

        [Fact]
        public async Task GetByIdAsync_NoRateResolved_ConvertedAmountIsNull()
        {
            var expense = MakeExpense(currencyId: 1);
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetByIdAsync(1, 42)).ReturnsAsync(expense);
            var rateService = new Mock<ICurrencyRateService>();
            rateService.Setup(r => r.ResolveRateAsync(1, 2, expense.Date)).ReturnsAsync((decimal?)null);

            var result = await CreateService(repo.Object, rateService.Object).GetByIdAsync(1, 42, displayCurrencyId: 2);

            Assert.Null(result!.ConvertedAmount);
            Assert.Null(result.DisplayCurrency);
        }

        [Fact]
        public async Task GetPagedAsync_DisplayCurrencySet_PopulatesConvertedAmountAndDisplayCurrencyPerExpense()
        {
            var expense = MakeExpense(currencyId: 1);
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetPagedAsync(It.IsAny<ExpenseFilterDto>(), 42))
                .ReturnsAsync((new List<Expense> { expense }, 1));
            var rateService = new Mock<ICurrencyRateService>();
            rateService.Setup(r => r.ResolveRateAsync(1, 3, expense.Date)).ReturnsAsync(1.1m);
            var currencyRepo = new Mock<ICurrencyRepository>();
            currencyRepo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(GbpCurrency);

            var filter = new ExpenseFilterDto { DisplayCurrencyId = 3 };
            var result = await CreateService(repo.Object, rateService.Object, currencyRepo.Object).GetPagedAsync(filter, 42);

            var item = result.Items.Single();
            Assert.Equal(110m, item.ConvertedAmount); // 100 * 1.1
            Assert.NotNull(item.DisplayCurrency);
            Assert.Equal("GBP", item.DisplayCurrency!.Code);
        }
    }
}
