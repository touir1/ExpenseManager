using com.touir.expenses.Users.Models;

namespace com.touir.expenses.Users.Services.Contracts
{
    public interface IRoleService
    {
        Task<IEnumerable<Role>> GetUserRolesByApplicationCodeAsync(string applicationCode, int userId);
    }
}
