using com.touir.expenses.Users.Models;
using com.touir.expenses.Users.Repositories.Contracts;
using com.touir.expenses.Users.Services.Contracts;

namespace com.touir.expenses.Users.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        public RoleService(IRoleRepository roleRepository) 
        {
            _roleRepository = roleRepository;
        }
        public async Task<IEnumerable<Role>> GetUserRolesByApplicationCodeAsync(string applicationCode, int userId)
        {
            if (applicationCode == null)
                return null;
            return await _roleRepository.GetUserRolesByApplicationCodeAsync(applicationCode, userId);
        }
    }
}
