using com.touir.expenses.Users.Models;

namespace com.touir.expenses.Users.Repositories.Contracts
{
    public interface IUserRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
    }
}
