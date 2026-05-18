using Microsoft.Extensions.Caching.Memory;
using Touir.ExpensesManager.Expenses.Models.Lookups;
using Touir.ExpensesManager.Expenses.Services;
using Touir.ExpensesManager.Expenses.Tests.TestHelpers;

namespace Touir.ExpensesManager.Expenses.Tests.Services
{
    public class LookupCacheServiceTests : IDisposable
    {
        private readonly TestExpensesDbContextWrapper _wrapper;
        private readonly MemoryCache _cache;
        private readonly LookupCacheService _sut;

        public LookupCacheServiceTests()
        {
            _wrapper = new TestExpensesDbContextWrapper();
            _cache = new MemoryCache(new MemoryCacheOptions());
            _sut = new LookupCacheService(_cache, _wrapper.Context);
        }

        public void Dispose()
        {
            _wrapper.Dispose();
            _cache.Dispose();
            GC.SuppressFinalize(this);
        }

        [Fact]
        public async Task GetIdAsync_ReturnsCorrectId_ForSeededEntry()
        {
            var id = await _sut.GetIdAsync<OperationSource>("SingleWeb");
            Assert.Equal(1, id);
        }

        [Fact]
        public async Task GetIdAsync_ReturnsCorrectId_BulkWeb()
        {
            var id = await _sut.GetIdAsync<OperationSource>("BulkWeb");
            Assert.Equal(3, id);
        }

        [Fact]
        public async Task GetNameAsync_ReturnsCorrectName_ForSeededEntry()
        {
            var name = await _sut.GetNameAsync<OperationSource>(2);
            Assert.Equal("SingleMobile", name);
        }

        [Fact]
        public async Task GetIdAsync_ThrowsKeyNotFoundException_ForUnknownName()
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _sut.GetIdAsync<OperationSource>("DoesNotExist"));
        }

        [Fact]
        public async Task GetNameAsync_ThrowsKeyNotFoundException_ForUnknownId()
        {
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _sut.GetNameAsync<OperationSource>(999));
        }

        [Fact]
        public async Task GetIdAsync_SecondCall_ReturnsSameResult_FromCache()
        {
            var first = await _sut.GetIdAsync<FamilyRole>("Head");
            var second = await _sut.GetIdAsync<FamilyRole>("Head");
            Assert.Equal(first, second);
        }

        [Fact]
        public async Task GetNameAsync_SecondCall_ReturnsSameResult_FromCache()
        {
            var first = await _sut.GetNameAsync<FamilyRole>(1);
            var second = await _sut.GetNameAsync<FamilyRole>(1);
            Assert.Equal(first, second);
        }

        [Fact]
        public async Task GetIdAsync_WorksForAllLookupTypes()
        {
            Assert.Equal(1, await _sut.GetIdAsync<ModifiedSource>("Web"));
            Assert.Equal(2, await _sut.GetIdAsync<ModifiedSource>("Mobile"));
            Assert.Equal(1, await _sut.GetIdAsync<RateSource>("Auto"));
            Assert.Equal(2, await _sut.GetIdAsync<RateSource>("Manual"));
            Assert.Equal(1, await _sut.GetIdAsync<ConflictStatus>("Pending"));
            Assert.Equal(2, await _sut.GetIdAsync<ConflictStatus>("Resolved"));
            Assert.Equal(3, await _sut.GetIdAsync<ConflictResolution>("Custom"));
            Assert.Equal(1, await _sut.GetIdAsync<AuditOperation>("Add"));
            Assert.Equal(2, await _sut.GetIdAsync<AuditOperation>("Update"));
            Assert.Equal(3, await _sut.GetIdAsync<AuditOperation>("Delete"));
            Assert.Equal(1, await _sut.GetIdAsync<SnapshotType>("Before"));
            Assert.Equal(2, await _sut.GetIdAsync<SnapshotType>("After"));
        }
    }
}
