using Moq;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;
using Touir.ExpensesManager.Expenses.Services;

namespace Touir.ExpensesManager.Expenses.Tests.Services
{
    public class AdminCurrencyServiceTests
    {
        private static AdminCurrencyService CreateService(ICurrencyRepository? repo = null)
            => new(repo ?? Mock.Of<ICurrencyRepository>());

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
    }
}
