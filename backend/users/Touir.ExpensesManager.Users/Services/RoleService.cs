using Touir.ExpensesManager.Users.Controllers.EO;
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
        public async Task<IEnumerable<RoleEo>> GetUserRolesByApplicationCodeAsync(string applicationCode, int userId)
        {
            if (applicationCode == null)
                return null;
            var roles = await _roleRepository.GetUserRolesByApplicationCodeAsync(applicationCode, userId);
            return roles?.Select(r => new RoleEo
            {
                Id = r.Id,
                Code = r.Code,
                Name = r.Name,
                Description = r.Description,
                Application = r.Application != null ? new ApplicationEo
                {
                    Id = r.Application.Id,
                    Code = r.Application.Code,
                    Name = r.Application.Name,
                    Description = r.Application.Description,
                    UrlPath = r.Application.UrlPath
                } : null
            });
        }
    }
}
