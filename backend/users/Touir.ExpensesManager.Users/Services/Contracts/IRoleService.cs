using Touir.ExpensesManager.Users.Models;

namespace Touir.ExpensesManager.Users.Services.Contracts
{
    public interface IRoleService
    {
        Task<IEnumerable<Role>> GetUserRolesByApplicationCodeAsync(string applicationCode, int userId);
    }
}
