using com.touir.expenses.Users.Models;

namespace com.touir.expenses.Users.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
    }
}
