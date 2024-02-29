using com.touir.expenses.Users.Models;

namespace com.touir.expenses.Users.Repositories.Contracts
{
    public interface IRoleRepository
    {
        Task<IEnumerable<Role>> GetUserRolesByApplicationCodeAsync(string applicationCode, int userId);
    }
}
