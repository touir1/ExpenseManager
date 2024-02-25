using com.touir.expenses.Expenses.Models;

namespace com.touir.expenses.Expenses.Repositories
{
    public interface IUserRepository
    {
        Task SaveOrUpdateUserAsync(User user);
        Task DeleteUserAsync(User user);
        Task<User?> GetUserByIdAsync(int id);
        Task<IEnumerable<User>> GetUsersByFamilyIdAsync(int familyId);
    }
}
