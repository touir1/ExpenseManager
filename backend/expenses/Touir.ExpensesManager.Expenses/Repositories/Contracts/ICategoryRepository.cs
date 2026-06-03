using Touir.ExpensesManager.Expenses.Models;

namespace Touir.ExpensesManager.Expenses.Repositories.Contracts
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllActiveAsync();
        Task<IEnumerable<Category>> GetAllWithArchivedAsync();
        Task<Category?> GetByIdAsync(int id);
        Task<bool> ExistsWithNameAsync(string name, int? parentCategoryId, int? excludeId = null);
        Task<Category> AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task ArchiveAsync(int id);
        Task UnarchiveAsync(int id);
    }
}
