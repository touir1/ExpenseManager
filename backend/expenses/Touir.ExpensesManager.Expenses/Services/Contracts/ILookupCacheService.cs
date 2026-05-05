using Touir.ExpensesManager.Expenses.Models.Lookups;

namespace Touir.ExpensesManager.Expenses.Services.Contracts
{
    public interface ILookupCacheService
    {
        Task<int> GetIdAsync<T>(string name) where T : class, ILookupEntity;
        Task<string> GetNameAsync<T>(int id) where T : class, ILookupEntity;
    }
}
