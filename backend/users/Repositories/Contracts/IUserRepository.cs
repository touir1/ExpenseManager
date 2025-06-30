using com.touir.expenses.Users.Models;

namespace com.touir.expenses.Users.Repositories.Contracts
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
