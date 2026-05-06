using Moq;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;
using Touir.ExpensesManager.Expenses.Services;

namespace Touir.ExpensesManager.Expenses.Tests.Services
{
    public class CurrencyServiceTests
    {
        private static CurrencyService CreateService(ICurrencyRepository? repo = null)
        {
            return new CurrencyService(repo ?? Mock.Of<ICurrencyRepository>());
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllCurrencies()
        {
            var currencies = new List<Currency>
            {
                new() { Id = 1, Code = "USD", Name = "US Dollar", Symbol = "$", Decimals = 2 },
                new() { Id = 2, Code = "EUR", Name = "Euro", Symbol = "€", Decimals = 2 }
            };
            var mockRepo = new Mock<ICurrencyRepository>();
            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(currencies);

            var result = (await CreateService(mockRepo.Object).GetAllAsync()).ToList();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetAllAsync_MapsAllFieldsCorrectly()
        {
            var currencies = new List<Currency>
            {
                new() { Id = 5, Code = "TND", Name = "Tunisian Dinar", Symbol = "د.ت", Decimals = 3 }
            };
            var mockRepo = new Mock<ICurrencyRepository>();
            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(currencies);

            var result = (await CreateService(mockRepo.Object).GetAllAsync()).ToList();

            var currency = result.First(c => c.Code == "TND");
            Assert.Equal("Tunisian Dinar", currency.Name);
            Assert.Equal("د.ت", currency.Symbol);
            Assert.Equal(3, currency.Decimals);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEmptyWhenNoCurrenciesExist()
        {
            var mockRepo = new Mock<ICurrencyRepository>();
            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync([]);

            var result = await CreateService(mockRepo.Object).GetAllAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_MapsIdCorrectly()
        {
            var currencies = new List<Currency>
            {
                new() { Id = 7, Code = "GBP", Name = "British Pound", Symbol = "£", Decimals = 2 }
            };
            var mockRepo = new Mock<ICurrencyRepository>();
            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(currencies);

            var result = (await CreateService(mockRepo.Object).GetAllAsync()).ToList();

            Assert.Equal(7, result[0].Id);
        }

        [Fact]
        public async Task GetAllAsync_CallsRepositoryOnce()
        {
            var mockRepo = new Mock<ICurrencyRepository>();
            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync([]);

            await CreateService(mockRepo.Object).GetAllAsync();

            mockRepo.Verify(r => r.GetAllAsync(), Times.Once);
        }
    }
}
