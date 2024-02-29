using com.touir.expenses.Users.Models;

namespace com.touir.expenses.Users.Repositories.Contracts
{
    public interface IAuthenticationRepository
    {
        Task<Authentication?> GetAuthenticationByIdAsync(int id);
    }
}
