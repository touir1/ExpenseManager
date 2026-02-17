using Touir.ExpensesManager.Expenses.Models;

namespace Touir.ExpensesManager.Expenses.Repositories
{
    public interface IUserRepository
    {
        Task SaveOrUpdateUserAsync(User user);
        Task DeleteUserAsync(User user);
        Task<User?> GetUserByIdAsync(int id);
        Task<IEnumerable<User>> GetUsersByFamilyIdAsync(int familyId);
    }
}
