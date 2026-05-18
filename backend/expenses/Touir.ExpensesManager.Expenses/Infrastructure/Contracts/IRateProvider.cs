namespace Touir.ExpensesManager.Expenses.Infrastructure.Contracts
{
    public interface IRateProvider
    {
        Task<Dictionary<string, decimal>> FetchRatesAsync(string sourceCurrencyCode, DateOnly date, CancellationToken ct = default);
        Task<Dictionary<DateOnly, Dictionary<string, decimal>>> FetchRatesRangeAsync(string sourceCurrencyCode, DateOnly from, DateOnly to, CancellationToken ct = default);
    }
}
