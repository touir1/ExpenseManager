using Touir.ExpensesManager.Users.Controllers.DTO;

namespace Touir.ExpensesManager.Users.Services.Contracts
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleDto>> GetUserRolesByApplicationCodeAsync(string applicationCode, int userId);
    }
}
