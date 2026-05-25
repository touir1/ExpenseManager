using Moq;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;
using Touir.ExpensesManager.Expenses.Services;

namespace Touir.ExpensesManager.Expenses.Tests.Services
{
    public class UserConfigServiceTests
    {
        private static UserConfigService CreateService(
            IUserConfigRepository? configRepo = null,
            ICurrencyRepository? currencyRepo = null)
        {
            return new UserConfigService(
                configRepo ?? Mock.Of<IUserConfigRepository>(),
                currencyRepo ?? Mock.Of<ICurrencyRepository>());
        }

        // ── GetAsync ──────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAsync_ReturnsNullFields_WhenNoConfigRow()
        {
            var configRepo = new Mock<IUserConfigRepository>();
            configRepo.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync((UserConfig?)null);

            var result = await CreateService(configRepo.Object).GetAsync(1);

            Assert.Null(result.DefaultCurrencyId);
            Assert.Null(result.DefaultCurrency);
        }

        [Fact]
        public async Task GetAsync_ReturnsPopulatedDto_WhenConfigRowExists()
        {
            var currency = new Currency { Id = 2, Code = "EUR", Name = "Euro", Symbol = "€", Decimals = 2 };
            var config = new UserConfig { Id = 1, UserId = 1, DefaultCurrencyId = 2, DefaultCurrency = currency };
            var configRepo = new Mock<IUserConfigRepository>();
            configRepo.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(config);

            var result = await CreateService(configRepo.Object).GetAsync(1);

            Assert.Equal(2, result.DefaultCurrencyId);
            Assert.NotNull(result.DefaultCurrency);
            Assert.Equal("EUR", result.DefaultCurrency.Code);
        }

        [Fact]
        public async Task GetAsync_CallsRepositoryOnce()
        {
            var configRepo = new Mock<IUserConfigRepository>();
            configRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<int>())).ReturnsAsync((UserConfig?)null);

            await CreateService(configRepo.Object).GetAsync(1);

            configRepo.Verify(r => r.GetByUserIdAsync(1), Times.Once);
        }

        // ── UpdateAsync ───────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_ReturnsNull_WhenCurrencyIdIsInvalid()
        {
            var currencyRepo = new Mock<ICurrencyRepository>();
            currencyRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Currency?)null);

            var result = await CreateService(currencyRepo: currencyRepo.Object).UpdateAsync(1, 999);

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsDto_WhenCurrencyIdIsValid()
        {
            var currency = new Currency { Id = 3, Code = "USD", Name = "US Dollar", Symbol = "$", Decimals = 2 };
            var config = new UserConfig { Id = 1, UserId = 1, DefaultCurrencyId = 3, DefaultCurrency = currency };
            var currencyRepo = new Mock<ICurrencyRepository>();
            var configRepo = new Mock<IUserConfigRepository>();
            currencyRepo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(currency);
            configRepo.Setup(r => r.UpsertAsync(1, 3)).ReturnsAsync(config);

            var result = await CreateService(configRepo.Object, currencyRepo.Object).UpdateAsync(1, 3);

            Assert.NotNull(result);
            Assert.Equal(3, result.DefaultCurrencyId);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsDto_WhenClearingCurrencyWithNull()
        {
            var config = new UserConfig { Id = 1, UserId = 1, DefaultCurrencyId = null };
            var configRepo = new Mock<IUserConfigRepository>();
            configRepo.Setup(r => r.UpsertAsync(1, null)).ReturnsAsync(config);

            var result = await CreateService(configRepo.Object).UpdateAsync(1, null);

            Assert.NotNull(result);
            Assert.Null(result.DefaultCurrencyId);
        }

        [Fact]
        public async Task UpdateAsync_DoesNotCheckCurrency_WhenCurrencyIdIsNull()
        {
            var config = new UserConfig { Id = 1, UserId = 1, DefaultCurrencyId = null };
            var currencyRepo = new Mock<ICurrencyRepository>();
            var configRepo = new Mock<IUserConfigRepository>();
            configRepo.Setup(r => r.UpsertAsync(1, null)).ReturnsAsync(config);

            await CreateService(configRepo.Object, currencyRepo.Object).UpdateAsync(1, null);

            currencyRepo.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_CallsUpsert_WhenCurrencyIsValid()
        {
            var currency = new Currency { Id = 5, Code = "GBP", Name = "British Pound", Symbol = "£", Decimals = 2 };
            var config = new UserConfig { Id = 1, UserId = 1, DefaultCurrencyId = 5, DefaultCurrency = currency };
            var currencyRepo = new Mock<ICurrencyRepository>();
            var configRepo = new Mock<IUserConfigRepository>();
            currencyRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(currency);
            configRepo.Setup(r => r.UpsertAsync(1, 5)).ReturnsAsync(config);

            await CreateService(configRepo.Object, currencyRepo.Object).UpdateAsync(1, 5);

            configRepo.Verify(r => r.UpsertAsync(1, 5), Times.Once);
        }
    }
}
