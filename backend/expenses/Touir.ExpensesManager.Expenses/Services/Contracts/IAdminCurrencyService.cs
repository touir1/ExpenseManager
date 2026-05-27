using Touir.ExpensesManager.Expenses.Controllers.DTO;

namespace Touir.ExpensesManager.Expenses.Services.Contracts
{
    public interface IAdminCurrencyService
    {
        Task<CurrencyDto> AddCurrencyAsync(string code, string name, string symbol, int decimals);
    }
}
