using Touir.ExpensesManager.Expenses.Models;

namespace Touir.ExpensesManager.Expenses.Repositories.Contracts
{
    public interface ICurrencyRepository
    {
        Task<IEnumerable<Currency>> GetAllAsync();
        Task<Currency?> GetByIdAsync(int id);
        Task<bool> ExistsByCodeAsync(string code);
        Task<Currency> AddAsync(Currency currency);
    }
}
