using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Repositories;
using Touir.ExpensesManager.Expenses.Tests.TestHelpers;

namespace Touir.ExpensesManager.Expenses.Tests.Repositories
{
    public class UserConfigRepositoryTests : IDisposable
    {
        private readonly TestExpensesDbContextWrapper _wrapper;
        private readonly UserConfigRepository _sut;

        public UserConfigRepositoryTests()
        {
            _wrapper = new TestExpensesDbContextWrapper();
            _sut = new UserConfigRepository(_wrapper.Context);
        }

        public void Dispose()
        {
            _wrapper.Dispose();
            GC.SuppressFinalize(this);
        }

        // ── GetByUserIdAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetByUserIdAsync_ReturnsNull_WhenNoRowExists()
        {
            var result = await _sut.GetByUserIdAsync(999);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByUserIdAsync_ReturnsEntity_WhenRowExists()
        {
            _wrapper.Context.UserConfigs.Add(new UserConfig { UserId = 1, DefaultCurrencyId = null });
            await _wrapper.Context.SaveChangesAsync();

            var result = await _sut.GetByUserIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.UserId);
        }

        [Fact]
        public async Task GetByUserIdAsync_LoadsDefaultCurrency_WhenSet()
        {
            var currency = _wrapper.Context.Currencies.First();
            _wrapper.Context.UserConfigs.Add(new UserConfig { UserId = 2, DefaultCurrencyId = currency.Id });
            await _wrapper.Context.SaveChangesAsync();

            var result = await _sut.GetByUserIdAsync(2);

            Assert.NotNull(result);
            Assert.NotNull(result.DefaultCurrency);
            Assert.Equal(currency.Id, result.DefaultCurrency.Id);
        }

        [Fact]
        public async Task GetByUserIdAsync_ReturnsNull_ForDifferentUserId()
        {
            _wrapper.Context.UserConfigs.Add(new UserConfig { UserId = 10, DefaultCurrencyId = null });
            await _wrapper.Context.SaveChangesAsync();

            var result = await _sut.GetByUserIdAsync(11);

            Assert.Null(result);
        }

        // ── UpsertAsync ───────────────────────────────────────────────────────────

        [Fact]
        public async Task UpsertAsync_InsertsNewRow_WhenNoExistingConfig()
        {
            var result = await _sut.UpsertAsync(20, null);

            Assert.NotNull(result);
            Assert.Equal(20, result.UserId);
            Assert.Null(result.DefaultCurrencyId);

            var persisted = _wrapper.Context.UserConfigs.FirstOrDefault(c => c.UserId == 20);
            Assert.NotNull(persisted);
        }

        [Fact]
        public async Task UpsertAsync_SetsDefaultCurrencyId_WhenInserting()
        {
            var currency = _wrapper.Context.Currencies.First();

            var result = await _sut.UpsertAsync(21, currency.Id);

            Assert.Equal(currency.Id, result.DefaultCurrencyId);
        }

        [Fact]
        public async Task UpsertAsync_UpdatesExistingRow_WhenConfigExists()
        {
            _wrapper.Context.UserConfigs.Add(new UserConfig { UserId = 30, DefaultCurrencyId = null });
            await _wrapper.Context.SaveChangesAsync();

            var currency = _wrapper.Context.Currencies.First();
            var result = await _sut.UpsertAsync(30, currency.Id);

            Assert.Equal(currency.Id, result.DefaultCurrencyId);
        }

        [Fact]
        public async Task UpsertAsync_ClearsCurrencyId_WhenUpdatingWithNull()
        {
            var currency = _wrapper.Context.Currencies.First();
            _wrapper.Context.UserConfigs.Add(new UserConfig { UserId = 31, DefaultCurrencyId = currency.Id });
            await _wrapper.Context.SaveChangesAsync();

            var result = await _sut.UpsertAsync(31, null);

            Assert.Null(result.DefaultCurrencyId);
        }

        [Fact]
        public async Task UpsertAsync_DoesNotCreateDuplicateRow_WhenCalledTwice()
        {
            await _sut.UpsertAsync(40, null);
            await _sut.UpsertAsync(40, null);

            var count = _wrapper.Context.UserConfigs.Count(c => c.UserId == 40);
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task UpsertAsync_LoadsDefaultCurrencyNavProp_WhenCurrencyIdSet()
        {
            var currency = _wrapper.Context.Currencies.First();

            var result = await _sut.UpsertAsync(50, currency.Id);

            Assert.NotNull(result.DefaultCurrency);
            Assert.Equal(currency.Id, result.DefaultCurrency.Id);
        }
    }
}
