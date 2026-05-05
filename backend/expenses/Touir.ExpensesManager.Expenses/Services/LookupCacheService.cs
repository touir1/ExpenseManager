using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Touir.ExpensesManager.Expenses.Infrastructure;
using Touir.ExpensesManager.Expenses.Models.Lookups;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Services
{
    public class LookupCacheService : ILookupCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ExpensesDbContext _dbContext;

        private static readonly MemoryCacheEntryOptions CacheOptions =
            new MemoryCacheEntryOptions().SetPriority(CacheItemPriority.NeverRemove);

        public LookupCacheService(IMemoryCache cache, ExpensesDbContext dbContext)
        {
            _cache = cache;
            _dbContext = dbContext;
        }

        public async Task<int> GetIdAsync<T>(string name) where T : class, ILookupEntity
        {
            var byName = await GetByNameAsync<T>();
            if (!byName.TryGetValue(name, out var id))
                throw new KeyNotFoundException($"{typeof(T).Name} has no entry '{name}'.");
            return id;
        }

        public async Task<string> GetNameAsync<T>(int id) where T : class, ILookupEntity
        {
            var byId = await GetByIdAsync<T>();
            if (!byId.TryGetValue(id, out var name))
                throw new KeyNotFoundException($"{typeof(T).Name} has no entry with id {id}.");
            return name;
        }

        private async Task<IReadOnlyDictionary<string, int>> GetByNameAsync<T>() where T : class, ILookupEntity
        {
            var key = ByNameKey<T>();
            if (_cache.TryGetValue(key, out IReadOnlyDictionary<string, int>? cached) && cached != null)
                return cached;

            await LoadAsync<T>();
            return _cache.Get<IReadOnlyDictionary<string, int>>(key)!;
        }

        private async Task<IReadOnlyDictionary<int, string>> GetByIdAsync<T>() where T : class, ILookupEntity
        {
            var key = ByIdKey<T>();
            if (_cache.TryGetValue(key, out IReadOnlyDictionary<int, string>? cached) && cached != null)
                return cached;

            await LoadAsync<T>();
            return _cache.Get<IReadOnlyDictionary<int, string>>(key)!;
        }

        private async Task LoadAsync<T>() where T : class, ILookupEntity
        {
            var entries = await _dbContext.Set<T>().AsNoTracking().ToListAsync();
            _cache.Set(ByNameKey<T>(), (IReadOnlyDictionary<string, int>)entries.ToDictionary(e => e.Name, e => e.Id), CacheOptions);
            _cache.Set(ByIdKey<T>(), (IReadOnlyDictionary<int, string>)entries.ToDictionary(e => e.Id, e => e.Name), CacheOptions);
        }

        private static string ByNameKey<T>() => $"lookup:{typeof(T).Name}:byName";
        private static string ByIdKey<T>() => $"lookup:{typeof(T).Name}:byId";
    }
}
