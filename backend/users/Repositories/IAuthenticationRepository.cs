using com.touir.expenses.Users.Models;

namespace com.touir.expenses.Users.Repositories
{
    public interface IAuthenticationRepository
    {
        Task<Authentication?> GetAuthenticationByIdAsync(int id);
    }
}
