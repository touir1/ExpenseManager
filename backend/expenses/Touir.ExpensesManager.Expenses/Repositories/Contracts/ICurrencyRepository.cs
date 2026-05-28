using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Models;

namespace Touir.ExpensesManager.Expenses.Repositories.Contracts
{
    public interface ICurrencyRepository
    {
        Task<IEnumerable<Currency>> GetAllAsync();
        Task<Currency?> GetByIdAsync(int id);
        Task<bool> ExistsByCodeAsync(string code);
        Task<Currency> AddAsync(Currency currency);
        Task<Currency> UpdateAsync(Currency currency);
        Task DeleteAsync(int id);
        Task<IEnumerable<CurrencyDefaultRateDto>> GetDefaultsForSourceAsync(int sourceCurrencyId);
    }
}
