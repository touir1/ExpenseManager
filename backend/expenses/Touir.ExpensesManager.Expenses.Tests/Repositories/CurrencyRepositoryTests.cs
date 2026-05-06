using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Repositories;
using Touir.ExpensesManager.Expenses.Tests.TestHelpers;

namespace Touir.ExpensesManager.Expenses.Tests.Repositories
{
    public class CurrencyRepositoryTests : IDisposable
    {
        private readonly TestExpensesDbContextWrapper _wrapper;
        private readonly CurrencyRepository _sut;

        public CurrencyRepositoryTests()
        {
            _wrapper = new TestExpensesDbContextWrapper();
            _sut = new CurrencyRepository(_wrapper.Context);
        }

        public void Dispose()
        {
            _wrapper.Dispose();
            GC.SuppressFinalize(this);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllCurrencies()
        {
            _wrapper.Context.Currencies.AddRange(
                new Currency { Code = "USD", Name = "US Dollar", Symbol = "$", Decimals = 2 },
                new Currency { Code = "EUR", Name = "Euro", Symbol = "€", Decimals = 2 }
            );
            await _wrapper.Context.SaveChangesAsync();

            var result = (await _sut.GetAllAsync()).ToList();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetAllAsync_MapsAllFields()
        {
            _wrapper.Context.Currencies.Add(
                new Currency { Code = "TND", Name = "Tunisian Dinar", Symbol = "د.ت", Decimals = 3 }
            );
            await _wrapper.Context.SaveChangesAsync();

            var result = (await _sut.GetAllAsync()).ToList();

            var currency = result.First();
            Assert.Equal("TND", currency.Code);
            Assert.Equal("Tunisian Dinar", currency.Name);
            Assert.Equal("د.ت", currency.Symbol);
            Assert.Equal(3, currency.Decimals);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEmptyWhenNoCurrenciesExist()
        {
            var result = await _sut.GetAllAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_AssignsPositiveIds()
        {
            _wrapper.Context.Currencies.Add(
                new Currency { Code = "GBP", Name = "British Pound", Symbol = "£", Decimals = 2 }
            );
            await _wrapper.Context.SaveChangesAsync();

            var result = (await _sut.GetAllAsync()).ToList();

            Assert.All(result, c => Assert.True(c.Id > 0));
        }
    }
}
