using Touir.ExpensesManager.Users.Models;

namespace Touir.ExpensesManager.Users.Repositories.Contracts
{
    public interface IAuthenticationRepository
    {
        Task<Authentication?> GetAuthenticationByUserIdAsync(int id);
        Task<bool> CreateAuthenticationAsync(Authentication authentication, bool resetHash = false);
        Task<bool> UpdateAuthenticationAsync(Authentication authentication, bool resetHash = false);
    }
}
