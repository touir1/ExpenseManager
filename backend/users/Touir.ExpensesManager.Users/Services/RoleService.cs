using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Repositories.Contracts;
using Touir.ExpensesManager.Users.Services.Contracts;

namespace Touir.ExpensesManager.Users.Services
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
