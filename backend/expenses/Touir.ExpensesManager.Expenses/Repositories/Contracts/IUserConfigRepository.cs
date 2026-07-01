using Touir.ExpensesManager.Expenses.Models;

namespace Touir.ExpensesManager.Expenses.Repositories.Contracts
{
    public interface IUserConfigRepository
    {
        Task<UserConfig?> GetByUserIdAsync(int userId);
        Task<UserConfig> UpsertAsync(int userId, int? defaultCurrencyId, int? defaultCategoryId);
        Task<Dictionary<string, string>?> GetDefaultCsvColumnMappingAsync(int userId);
        Task<UserConfig> UpsertCsvColumnMappingAsync(int userId, Dictionary<string, string>? mapping);
    }
}
