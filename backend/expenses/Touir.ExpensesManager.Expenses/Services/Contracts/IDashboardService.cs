using Touir.ExpensesManager.Expenses.Controllers.DTO;

namespace Touir.ExpensesManager.Expenses.Services.Contracts
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetSummaryAsync(
            int userId, int? familyId, DateOnly dateFrom, DateOnly dateTo, int? displayCurrencyId);

        Task<IEnumerable<MonthlyBreakdownDto>> GetMonthlyAsync(
            int userId, int? familyId, DateOnly dateFrom, DateOnly dateTo, int? displayCurrencyId);

        Task<IEnumerable<CategoryBreakdownDto>> GetCategoriesAsync(
            int userId, int? familyId, DateOnly dateFrom, DateOnly dateTo, int? displayCurrencyId);

        Task<IEnumerable<SameMonthYearlyDto>> GetSameMonthAcrossYearsAsync(
            int userId, int? familyId, int month, int? displayCurrencyId);

        Task<IEnumerable<CurrencyBreakdownDto>> GetByCurrencyAsync(
            int userId, int? familyId, DateOnly dateFrom, DateOnly dateTo, int? displayCurrencyId);

        Task<ExpensePagedResult> GetRecentAsync(
            int userId, int? familyId, DateOnly? dateFrom, DateOnly? dateTo);
    }
}
