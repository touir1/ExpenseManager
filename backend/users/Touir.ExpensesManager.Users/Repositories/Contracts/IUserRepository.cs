using Touir.ExpensesManager.Users.Models;

namespace Touir.ExpensesManager.Users.Repositories.Contracts
{
    public interface IUserRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(int id);
        Task<User> CreateUserAsync(User user);
        Task<bool?> DeleteUserAsync(User user);
        Task<IList<string>> GetUsedEmailValidationHashesAsync();
        Task<bool> ValidateEmail(string emailVerificationHash, string email);
        Task<User?> ValidateEmailAsync(string emailVerificationHash, string email);
        Task UpdateEmailValidationHashAsync(int userId, string newHash, DateTime expiresAt);
        Task<(IEnumerable<User> Users, int Total)> GetPagedAsync(string? search, int page, int pageSize);
        Task<bool> DisableAsync(int userId);
        Task<bool> EnableAsync(int userId);
    }
}
