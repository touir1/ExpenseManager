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

        // ── GetByIdAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_ReturnsCurrency_WhenExists()
        {
            _wrapper.Context.Currencies.Add(new Currency { Code = "XTC", Name = "Test Coin", Symbol = "T", Decimals = 2 });
            await _wrapper.Context.SaveChangesAsync();
            var id = _wrapper.Context.Currencies.First(c => c.Code == "XTC").Id;

            var result = await _sut.GetByIdAsync(id);

            Assert.NotNull(result);
            Assert.Equal("XTC", result!.Code);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
        {
            var result = await _sut.GetByIdAsync(99999);

            Assert.Null(result);
        }

        // ── ExistsByCodeAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task ExistsByCodeAsync_ReturnsTrue_WhenCodeExists()
        {
            _wrapper.Context.Currencies.Add(new Currency { Code = "XEX", Name = "Exist Coin", Symbol = "E", Decimals = 2 });
            await _wrapper.Context.SaveChangesAsync();

            var result = await _sut.ExistsByCodeAsync("XEX");

            Assert.True(result);
        }

        [Fact]
        public async Task ExistsByCodeAsync_ReturnsFalse_WhenNotFound()
        {
            var result = await _sut.ExistsByCodeAsync("NOPE");

            Assert.False(result);
        }

        // ── AddAsync ─────────────────────────────────────────────────────────

        [Fact]
        public async Task AddAsync_PersistsAndReturnsCurrency()
        {
            var currency = new Currency { Code = "XAD", Name = "Added Coin", Symbol = "A", Decimals = 0 };

            var result = await _sut.AddAsync(currency);

            Assert.True(result.Id > 0);
            Assert.NotNull(_wrapper.Context.Currencies.Find(result.Id));
        }

        // ── UpdateAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_PersistsChanges()
        {
            var currency = new Currency { Code = "XUP", Name = "Old Name", Symbol = "U", Decimals = 2 };
            _wrapper.Context.Currencies.Add(currency);
            await _wrapper.Context.SaveChangesAsync();

            currency.Name = "New Name";
            var result = await _sut.UpdateAsync(currency);

            Assert.Equal("New Name", result.Name);
            var stored = _wrapper.Context.Currencies.Find(currency.Id);
            Assert.Equal("New Name", stored!.Name);
        }

        // ── DeleteAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteAsync_RemovesCurrency_WhenExists()
        {
            var currency = new Currency { Code = "XDL", Name = "Del Coin", Symbol = "D", Decimals = 2 };
            _wrapper.Context.Currencies.Add(currency);
            await _wrapper.Context.SaveChangesAsync();
            var id = currency.Id;

            await _sut.DeleteAsync(id);

            Assert.Null(_wrapper.Context.Currencies.Find(id));
        }

        [Fact]
        public async Task DeleteAsync_DoesNotThrow_WhenNotFound()
        {
            var ex = await Record.ExceptionAsync(() => _sut.DeleteAsync(99999));
            Assert.Null(ex);
        }

        // ── GetDefaultsForSourceAsync ─────────────────────────────────────────

        [Fact]
        public async Task GetDefaultsForSourceAsync_ReturnsEmpty_WhenNoPairDefaults()
        {
            var src = new Currency { Code = "XSR", Name = "Source Coin", Symbol = "S", Decimals = 2 };
            _wrapper.Context.Currencies.Add(src);
            await _wrapper.Context.SaveChangesAsync();

            var result = (await _sut.GetDefaultsForSourceAsync(src.Id)).ToList();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetDefaultsForSourceAsync_ReturnsDefaults_WithAutoRate()
        {
            var src = new Currency { Code = "XS2", Name = "Source 2", Symbol = "S2", Decimals = 2 };
            var dst = new Currency { Code = "XD2", Name = "Dest 2", Symbol = "D2", Decimals = 2 };
            _wrapper.Context.Currencies.AddRange(src, dst);
            await _wrapper.Context.SaveChangesAsync();

            _wrapper.Context.CurrencyPairDefaults.Add(new CurrencyPairDefault
            {
                SourceCurrencyId = src.Id, DestinationCurrencyId = dst.Id, Rate = 1.5m
            });
            _wrapper.Context.CurrencyDailyRates.Add(new CurrencyDailyRate
            {
                SourceCurrencyId = src.Id, DestinationCurrencyId = dst.Id,
                Date = new DateOnly(2024, 1, 1), Rate = 1.55m, RateSourceId = 1
            });
            await _wrapper.Context.SaveChangesAsync();

            var result = (await _sut.GetDefaultsForSourceAsync(src.Id)).ToList();

            Assert.Single(result);
            Assert.Equal(dst.Id, result[0].DestinationCurrencyId);
            Assert.Equal(1.5m, result[0].DefaultRate);
            Assert.Equal(1.55m, result[0].LastAutoRate);
        }

        [Fact]
        public async Task GetDefaultsForSourceAsync_ReturnsDefaults_WithoutAutoRate()
        {
            var src = new Currency { Code = "XS3", Name = "Source 3", Symbol = "S3", Decimals = 2 };
            var dst = new Currency { Code = "XD3", Name = "Dest 3", Symbol = "D3", Decimals = 2 };
            _wrapper.Context.Currencies.AddRange(src, dst);
            await _wrapper.Context.SaveChangesAsync();

            _wrapper.Context.CurrencyPairDefaults.Add(new CurrencyPairDefault
            {
                SourceCurrencyId = src.Id, DestinationCurrencyId = dst.Id, Rate = 2.0m
            });
            await _wrapper.Context.SaveChangesAsync();

            var result = (await _sut.GetDefaultsForSourceAsync(src.Id)).ToList();

            Assert.Single(result);
            Assert.Equal(2.0m, result[0].DefaultRate);
            Assert.Null(result[0].LastAutoRate);
        }
    }
}
