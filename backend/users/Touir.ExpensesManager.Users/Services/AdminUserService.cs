using System.Text.Json;
using Touir.ExpensesManager.Users.Controllers.DTO;
using Touir.ExpensesManager.Users.Messaging.Messages;
using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Repositories.Contracts;
using Touir.ExpensesManager.Users.Services.Contracts;

namespace Touir.ExpensesManager.Users.Services
{
    public class AdminUserService : IAdminUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IOutboxRepository _outboxRepository;

        public AdminUserService(IUserRepository userRepository, IRoleRepository roleRepository, IOutboxRepository outboxRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _outboxRepository = outboxRepository;
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
            var roleIdList = roleIds.ToList();
            if (adminUserId == userId)
            {
                var allRoles = await _roleRepository.GetAllAsync();
                var appAdminRole = allRoles.FirstOrDefault(r => r.Code == "APP_ADMIN");
                if (appAdminRole != null && !roleIdList.Contains(appAdminRole.Id))
                    throw new InvalidOperationException("CANNOT_REMOVE_OWN_ADMIN_ROLE");
            }
            await _roleRepository.RemoveUserRolesAsync(userId);
            foreach (var roleId in roleIdList)
                await _roleRepository.AssignRoleToUserAsync(roleId, userId, adminUserId);

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user != null)
            {
                var isAdmin = await _roleRepository.IsAdminAsync(userId);
                await _outboxRepository.EnqueueAsync(new OutboxEvent
                {
                    MessageId = Guid.NewGuid().ToString(),
                    EventType = UserEventType.Updated,
                    Payload = JsonSerializer.Serialize(new UserEventMessage
                    {
                        EventType = UserEventType.Updated,
                        UserId = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        FamilyId = user.FamilyId,
                        IsAdmin = isAdmin
                    }),
                    CreatedAt = DateTime.UtcNow,
                    RetryCount = 0
                });
            }
        }
    }
}
