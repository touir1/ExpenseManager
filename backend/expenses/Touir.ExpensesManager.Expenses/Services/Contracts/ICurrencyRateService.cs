using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Requests;

namespace Touir.ExpensesManager.Expenses.Services.Contracts
{
    public interface ICurrencyRateService
    {
        Task<decimal?> ResolveRateAsync(int sourceCurrencyId, int destCurrencyId, DateOnly date);
        Task<IEnumerable<RateDto>> GetRateHistoryAsync(int sourceCurrencyId, int destCurrencyId);
        Task<RateDto> AddManualRateAsync(AddRateRequest request, int adminUserId);
        Task BulkAddManualRatesAsync(BulkAddRatesRequest request, int adminUserId);
        Task SetDefaultFallbackAsync(SetDefaultRateRequest request, int adminUserId);
        Task ResolveConflictAsync(int conflictId, ResolveConflictRequest request, int adminUserId);
        Task<IEnumerable<RateConflictDto>> GetPendingConflictsAsync();
        Task RunDailyUpdateAsync(CancellationToken ct = default);
        Task RefreshRatesFromAsync(DateOnly from, int? sourceCurrencyId = null, int? destinationCurrencyId = null, CancellationToken ct = default);
    }
}
