using Touir.ExpensesManager.Users.Controllers.EO;

namespace Touir.ExpensesManager.Users.Services.Contracts
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleEo>> GetUserRolesByApplicationCodeAsync(string applicationCode, int userId);
    }
}
