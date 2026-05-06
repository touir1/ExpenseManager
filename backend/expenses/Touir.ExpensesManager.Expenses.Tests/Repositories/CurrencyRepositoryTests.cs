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
                new Currency { Code = "ZZ1", Name = "Test Currency 1", Symbol = "$", Decimals = 2 },
                new Currency { Code = "ZZ2", Name = "Test Currency 2", Symbol = "$", Decimals = 2 }
            );
            await _wrapper.Context.SaveChangesAsync();

            var result = (await _sut.GetAllAsync()).ToList();

            Assert.Equal(156, result.Count); // 154 seeded + 2 added
        }

        [Fact]
        public async Task GetAllAsync_MapsAllFields()
        {
            _wrapper.Context.Currencies.Add(
                new Currency { Code = "ZZZ", Name = "Zero Zone Currency", Symbol = "Ø", Decimals = 3 }
            );
            await _wrapper.Context.SaveChangesAsync();

            var result = (await _sut.GetAllAsync()).ToList();

            var currency = result.First(c => c.Code == "ZZZ");
            Assert.Equal("ZZZ", currency.Code);
            Assert.Equal("Zero Zone Currency", currency.Name);
            Assert.Equal("Ø", currency.Symbol);
            Assert.Equal(3, currency.Decimals);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsSeededCurrenciesWithNoAdded()
        {
            var result = await _sut.GetAllAsync();

            Assert.Equal(154, result.Count());
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
