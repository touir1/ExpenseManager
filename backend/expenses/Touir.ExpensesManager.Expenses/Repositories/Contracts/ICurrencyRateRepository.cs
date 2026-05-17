using Touir.ExpensesManager.Expenses.Models;

namespace Touir.ExpensesManager.Expenses.Repositories.Contracts
{
    public interface ICurrencyRateRepository
    {
        Task<CurrencyDailyRate?> GetExactAsync(int sourceId, int destId, DateOnly date);
        Task<CurrencyDailyRate?> GetMostRecentBeforeAsync(int sourceId, int destId, DateOnly date);
        Task<CurrencyPairDefault?> GetDefaultAsync(int sourceId, int destId);
        Task<IEnumerable<CurrencyDailyRate>> GetHistoryAsync(int sourceId, int destId);
        Task AddRateAsync(CurrencyDailyRate rate);
        Task UpdateRateAsync(CurrencyDailyRate rate);
        Task AddConflictAsync(CurrencyRateConflict conflict);
        Task<IEnumerable<CurrencyRateConflict>> GetPendingConflictsAsync();
        Task<CurrencyRateConflict?> GetConflictByIdAsync(int id);
        Task UpdateConflictAsync(CurrencyRateConflict conflict);
        Task SetDefaultAsync(CurrencyPairDefault pair);
    }
}
