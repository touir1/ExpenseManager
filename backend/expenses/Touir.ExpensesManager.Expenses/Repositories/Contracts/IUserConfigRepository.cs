using Touir.ExpensesManager.Expenses.Models;

namespace Touir.ExpensesManager.Expenses.Repositories.Contracts
{
    public interface IUserConfigRepository
    {
        Task<UserConfig?> GetByUserIdAsync(int userId);
        Task<UserConfig> UpsertAsync(int userId, int? defaultCurrencyId);
    }
}
