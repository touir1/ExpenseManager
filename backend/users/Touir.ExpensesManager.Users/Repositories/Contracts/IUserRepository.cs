using Touir.ExpensesManager.Users.Models;

namespace Touir.ExpensesManager.Users.Repositories.Contracts
{
    public interface IUserRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> CreateUserAsync(User user);
        Task<bool?> DeleteUserAsync(User user);
        Task<IList<string>> GetUsedEmailValidationHashesAsync();
        Task<bool> ValidateEmail(string emailValidationHash, string email);
    }
}
