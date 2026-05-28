using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Requests;

namespace Touir.ExpensesManager.Expenses.Services.Contracts
{
    public interface ICurrencyRateService
    {
        Task<decimal?> ResolveRateAsync(int sourceCurrencyId, int destCurrencyId, DateOnly date);
        Task<PagedRatesResponse> GetRateHistoryAsync(int? sourceCurrencyId, int? destCurrencyId, int page = 1, int pageSize = 50);
        Task<RateDto> AddManualRateAsync(AddRateRequest request, int adminUserId);
        Task BulkAddManualRatesAsync(BulkAddRatesRequest request, int adminUserId);
        Task SetDefaultFallbackAsync(SetDefaultRateRequest request, int adminUserId);
        Task ResolveConflictAsync(int conflictId, ResolveConflictRequest request, int adminUserId);
        Task<IEnumerable<RateConflictDto>> GetPendingConflictsAsync();
        Task RunDailyUpdateAsync(CancellationToken ct = default);
        Task RefreshRatesFromAsync(DateOnly from, DateOnly? to = null, int? sourceCurrencyId = null, int? destinationCurrencyId = null, CancellationToken ct = default);
    }
}
