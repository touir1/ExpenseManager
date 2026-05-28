using Moq;
using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;
using Touir.ExpensesManager.Expenses.Services;

namespace Touir.ExpensesManager.Expenses.Tests.Services
{
    public class AdminCurrencyServiceTests
    {
        private static AdminCurrencyService CreateService(
            ICurrencyRepository? repo = null,
            IExpenseRepository? expenseRepo = null,
            ICurrencyRateRepository? rateRepo = null)
            => new(
                repo ?? Mock.Of<ICurrencyRepository>(),
                expenseRepo ?? Mock.Of<IExpenseRepository>(),
                rateRepo ?? Mock.Of<ICurrencyRateRepository>());

        [Fact]
        public async Task AddCurrencyAsync_ReturnsDto_WhenCodeIsNew()
        {
            var added = new Currency { Id = 1, Code = "JPY", Name = "Japanese Yen", Symbol = "¥", Decimals = 0 };
            var repo = new Mock<ICurrencyRepository>();
            repo.Setup(r => r.ExistsByCodeAsync("JPY")).ReturnsAsync(false);
            repo.Setup(r => r.AddAsync(It.IsAny<Currency>())).ReturnsAsync(added);

            var result = await CreateService(repo.Object).AddCurrencyAsync("jpy", "Japanese Yen", "¥", 0);

            Assert.Equal(1, result.Id);
            Assert.Equal("JPY", result.Code);
        }

        [Fact]
        public async Task AddCurrencyAsync_NormalizesCodeToUpperCase()
        {
            var added = new Currency { Id = 2, Code = "GBP", Name = "Pound", Symbol = "£", Decimals = 2 };
            var repo = new Mock<ICurrencyRepository>();
            repo.Setup(r => r.ExistsByCodeAsync("GBP")).ReturnsAsync(false);
            repo.Setup(r => r.AddAsync(It.IsAny<Currency>())).ReturnsAsync(added);

            await CreateService(repo.Object).AddCurrencyAsync("gbp", "Pound", "£", 2);

            repo.Verify(r => r.ExistsByCodeAsync("GBP"), Times.Once);
        }

        [Fact]
        public async Task AddCurrencyAsync_Throws_WhenCodeAlreadyExists()
        {
            var repo = new Mock<ICurrencyRepository>();
            repo.Setup(r => r.ExistsByCodeAsync("USD")).ReturnsAsync(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                CreateService(repo.Object).AddCurrencyAsync("USD", "Dollar", "$", 2));
        }

        [Fact]
        public async Task AddCurrencyAsync_DoesNotCallAdd_WhenCodeExists()
        {
            var repo = new Mock<ICurrencyRepository>();
            repo.Setup(r => r.ExistsByCodeAsync("EUR")).ReturnsAsync(true);

            try { await CreateService(repo.Object).AddCurrencyAsync("EUR", "Euro", "€", 2); } catch { }

            repo.Verify(r => r.AddAsync(It.IsAny<Currency>()), Times.Never);
        }

        [Fact]
        public async Task UpdateCurrencyAsync_ReturnsMappedDto()
        {
            var currency = new Currency { Id = 1, Code = "EUR", Name = "Euro", Symbol = "€", Decimals = 2 };
            var repo = new Mock<ICurrencyRepository>();
            repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(currency);
            repo.Setup(r => r.UpdateAsync(It.IsAny<Currency>())).ReturnsAsync((Currency c) => c);

            var result = await CreateService(repo.Object).UpdateCurrencyAsync(1, "Euro Updated", "€", 2);

            Assert.Equal("Euro Updated", result.Name);
        }

        [Fact]
        public async Task UpdateCurrencyAsync_ThrowsWhenNotFound()
        {
            var repo = new Mock<ICurrencyRepository>();
            repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Currency?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                CreateService(repo.Object).UpdateCurrencyAsync(99, "X", "x", 2));
        }

        [Fact]
        public async Task DeleteCurrencyAsync_Succeeds_WhenNotInUse()
        {
            var currency = new Currency { Id = 1, Code = "JPY", Name = "Yen", Symbol = "¥", Decimals = 0 };
            var repo = new Mock<ICurrencyRepository>();
            repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(currency);

            var expRepo = new Mock<IExpenseRepository>();
            expRepo.Setup(r => r.GetDistinctCurrencyIdsAsync()).ReturnsAsync([]);

            var rateRepo = new Mock<ICurrencyRateRepository>();
            rateRepo.Setup(r => r.IsUsedInRatesAsync(1)).ReturnsAsync(false);

            await CreateService(repo.Object, expRepo.Object, rateRepo.Object).DeleteCurrencyAsync(1);

            repo.Verify(r => r.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeleteCurrencyAsync_ThrowsWhenInUse_InRates()
        {
            var currency = new Currency { Id = 1, Code = "EUR", Name = "Euro", Symbol = "€", Decimals = 2 };
            var repo = new Mock<ICurrencyRepository>();
            repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(currency);

            var expRepo = new Mock<IExpenseRepository>();
            expRepo.Setup(r => r.GetDistinctCurrencyIdsAsync()).ReturnsAsync([]);

            var rateRepo = new Mock<ICurrencyRateRepository>();
            rateRepo.Setup(r => r.IsUsedInRatesAsync(1)).ReturnsAsync(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                CreateService(repo.Object, expRepo.Object, rateRepo.Object).DeleteCurrencyAsync(1));
        }

        [Fact]
        public async Task GetCurrencyDefaultsAsync_DelegatesToRepo()
        {
            IEnumerable<CurrencyDefaultRateDto> defaults = [new CurrencyDefaultRateDto { DestinationCurrencyId = 2, DestinationCode = "USD", DefaultRate = 1.1m }];
            var repo = new Mock<ICurrencyRepository>();
            repo.Setup(r => r.GetDefaultsForSourceAsync(1)).ReturnsAsync(defaults);

            var result = (await CreateService(repo.Object).GetCurrencyDefaultsAsync(1)).ToList();

            Assert.Single(result);
            Assert.Equal(2, result[0].DestinationCurrencyId);
        }
    }
}
