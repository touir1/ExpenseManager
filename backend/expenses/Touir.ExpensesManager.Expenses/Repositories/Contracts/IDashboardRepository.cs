namespace Touir.ExpensesManager.Expenses.Repositories.Contracts
{
    public record CurrencyTotalRow(int CurrencyId, decimal Amount, int Count);
    public record CategoryTotalRow(int? CategoryId, int? SubcategoryId, int CurrencyId, decimal Amount);
    public record MonthlyTotalRow(int Year, int Month, int CurrencyId, decimal Amount);
    public record MonthlyCategoryTotalRow(int Year, int Month, int? CategoryId, int CurrencyId, decimal Amount);
    public record YearlyTotalRow(int Year, int CurrencyId, decimal Amount);

    public interface IDashboardRepository
    {
        Task<IEnumerable<CurrencyTotalRow>> GetTotalsAsync(
            int userId, int? familyId, DateOnly dateFrom, DateOnly dateTo);

        Task<IEnumerable<CategoryTotalRow>> GetCategoryTotalsAsync(
            int userId, int? familyId, DateOnly dateFrom, DateOnly dateTo);

        Task<IEnumerable<MonthlyTotalRow>> GetMonthlyTotalsAsync(
            int userId, int? familyId, DateOnly dateFrom, DateOnly dateTo);

        Task<IEnumerable<MonthlyCategoryTotalRow>> GetMonthlyCategoryTotalsAsync(
            int userId, int? familyId, DateOnly dateFrom, DateOnly dateTo);

        Task<IEnumerable<YearlyTotalRow>> GetYearlyTotalsForMonthAsync(
            int userId, int? familyId, int month);
    }
}
