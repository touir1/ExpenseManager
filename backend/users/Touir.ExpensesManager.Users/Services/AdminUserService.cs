using Touir.ExpensesManager.Users.Controllers.DTO;
using Touir.ExpensesManager.Users.Repositories.Contracts;
using Touir.ExpensesManager.Users.Services.Contracts;

namespace Touir.ExpensesManager.Users.Services
{
    public class AdminUserService : IAdminUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;

        public AdminUserService(IUserRepository userRepository, IRoleRepository roleRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
        }

        public async Task<(IEnumerable<AdminUserDto> Users, int Total)> GetUsersPagedAsync(string? search, int page, int pageSize)
        {
            var (users, total) = await _userRepository.GetPagedAsync(search, page, pageSize);
            var dtos = users.Select(u => new AdminUserDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                IsDisabled = u.IsDisabled,
                IsDeleted = u.IsDeleted,
                IsEmailValidated = u.IsEmailValidated,
                CreatedAt = u.CreatedAt,
                Roles = u.UserRoles.Select(ur => new RoleDto
                {
                    Id = ur.Role.Id,
                    Code = ur.Role.Code,
                    Name = ur.Role.Name,
                    Description = ur.Role.Description
                })
            });
            return (dtos, total);
        }

        public async Task<bool> DisableUserAsync(int userId)
        {
            return await _userRepository.DisableAsync(userId);
        }

        public async Task<bool> EnableUserAsync(int userId)
        {
            return await _userRepository.EnableAsync(userId);
        }

        public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
        {
            var roles = await _roleRepository.GetAllAsync();
            return roles.Select(r => new RoleDto
            {
                Id = r.Id,
                Code = r.Code,
                Name = r.Name,
                Description = r.Description,
                Application = r.Application is null ? null : new ApplicationDto
                {
                    Id = r.Application.Id,
                    Code = r.Application.Code,
                    Name = r.Application.Name
                }
            });
        }

        public async Task SetUserRolesAsync(int userId, IEnumerable<int> roleIds, int adminUserId)
        {
            if (adminUserId == userId)
            {
                var allRoles = await _roleRepository.GetAllAsync();
                var appAdminRole = allRoles.FirstOrDefault(r => r.Code == "APP_ADMIN");
                if (appAdminRole != null && !roleIds.Contains(appAdminRole.Id))
                    throw new InvalidOperationException("CANNOT_REMOVE_OWN_ADMIN_ROLE");
            }
            await _roleRepository.RemoveUserRolesAsync(userId);
            foreach (var roleId in roleIds)
                await _roleRepository.AssignRoleToUserAsync(roleId, userId, adminUserId);
        }
    }
}
