using Touir.ExpensesManager.Expenses.Models.External;

namespace Touir.ExpensesManager.Expenses.Repositories.External.Contracts
{
    public interface IUserRepository
    {
        Task SaveOrUpdateUserAsync(User user);
        Task DeleteUserAsync(User user);
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<IEnumerable<User>> GetUsersByFamilyIdAsync(int familyId);
        Task<IEnumerable<int>> GetAdminUserIdsAsync();
    }
}
