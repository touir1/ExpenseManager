using com.touir.expenses.Users.Models;

namespace com.touir.expenses.Users.Repositories.Contracts
{
    public interface IAuthenticationRepository
    {
        Task<Authentication?> GetAuthenticationByIdAsync(int id);
        Task<bool> CreateAuthenticationAsync(Authentication authentication, bool resetHash = false);
        Task<bool> UpdateAuthenticationAsync(Authentication authentication, bool resetHash = false);
    }
}
