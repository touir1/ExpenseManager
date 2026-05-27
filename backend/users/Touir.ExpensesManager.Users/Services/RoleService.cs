using Touir.ExpensesManager.Users.Controllers.DTO;
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
        public async Task<bool> IsAdminAsync(int userId)
        {
            return await _roleRepository.IsAdminAsync(userId);
        }

        public async Task<IEnumerable<RoleDto>> GetUserRolesByApplicationCodeAsync(string applicationCode, int userId)
        {
            if (applicationCode == null)
                return Enumerable.Empty<RoleDto>();
            var roles = await _roleRepository.GetUserRolesByApplicationCodeAsync(applicationCode, userId);
            if (roles == null)
                return Enumerable.Empty<RoleDto>();
            return roles.Select(r => new RoleDto
            {
                Id = r.Id,
                Code = r.Code,
                Name = r.Name,
                Description = r.Description,
                Application = r.Application != null ? new ApplicationDto
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
