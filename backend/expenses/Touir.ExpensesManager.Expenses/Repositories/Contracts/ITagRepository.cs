using Touir.ExpensesManager.Expenses.Models;

namespace Touir.ExpensesManager.Expenses.Repositories.Contracts
{
    public interface ITagRepository
    {
        Task<IEnumerable<Tag>> GetOwnAsync(int userId);
        Task<IEnumerable<Tag>> GetFamilyAsync(int userId);
        Task<IEnumerable<Tag>> GetByIdsAsync(IEnumerable<int> ids);
        Task<Tag?> GetByNameAsync(string name);
        Task<Tag> AddAsync(Tag tag);
        Task<bool> EnsureUserTagAsync(int userId, int tagId);
        Task<bool> RemoveUserTagAsync(int userId, int tagId);
        Task<bool> IsVisibleAsync(int userId, int tagId);
        Task SaveChangesAsync();
    }
}
