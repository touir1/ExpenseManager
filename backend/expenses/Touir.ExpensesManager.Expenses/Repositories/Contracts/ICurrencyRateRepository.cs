using Touir.ExpensesManager.Expenses.Models;

namespace Touir.ExpensesManager.Expenses.Repositories.Contracts
{
    public interface ICurrencyRateRepository
    {
        Task<CurrencyDailyRate?> GetExactAsync(int sourceId, int destId, DateOnly date);
        Task<CurrencyDailyRate?> GetMostRecentBeforeAsync(int sourceId, int destId, DateOnly date);
        Task<CurrencyPairDefault?> GetDefaultAsync(int sourceId, int destId);
        Task<(List<CurrencyDailyRate> Items, int Total)> GetHistoryAsync(int? sourceId, int? destId, int page, int pageSize);
        Task AddRateAsync(CurrencyDailyRate rate);
        Task UpdateRateAsync(CurrencyDailyRate rate);
        Task AddConflictAsync(CurrencyRateConflict conflict);
        Task<IEnumerable<CurrencyRateConflict>> GetPendingConflictsAsync();
        Task<CurrencyRateConflict?> GetConflictByIdAsync(int id);
        Task UpdateConflictAsync(CurrencyRateConflict conflict);
        Task SetDefaultAsync(CurrencyPairDefault pair);
        Task<Dictionary<int, (decimal Rate, int RateSourceId)>> GetExistingOnDateAsync(int sourceId, DateOnly date);
        Task<Dictionary<(int DestId, DateOnly Date), (decimal Rate, int RateSourceId)>> GetExistingInRangeAsync(int sourceId, DateOnly from, DateOnly to);
        Task<Dictionary<(int SrcId, int DestId, DateOnly Date), (decimal Rate, int RateSourceId)>> GetExistingForPairsAsync(IEnumerable<(int srcId, int destId, DateOnly date)> pairs);
        Task AddRatesBatchAsync(IEnumerable<CurrencyDailyRate> rates);
        Task AddConflictsBatchAsync(IEnumerable<CurrencyRateConflict> conflicts);
        Task<bool> IsUsedInRatesAsync(int currencyId);
    }
}
