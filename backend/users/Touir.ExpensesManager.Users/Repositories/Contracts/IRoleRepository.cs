using Touir.ExpensesManager.Users.Models;

namespace Touir.ExpensesManager.Users.Repositories.Contracts
{
    public interface IRoleRepository
    {
        Task<IEnumerable<Role>> GetUserRolesByApplicationCodeAsync(string applicationCode, int userId);
        Task<Role?> GetDefaultRoleByApplicationIdAsync(int applicationId);
        Task<bool> AssignRoleToUserAsync(int roleId, int userId, int? createdByUserId  = null);
    }
}
