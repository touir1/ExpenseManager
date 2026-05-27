using Touir.ExpensesManager.Users.Controllers.DTO;

namespace Touir.ExpensesManager.Users.Services.Contracts
{
    public interface IAdminUserService
    {
        Task<(IEnumerable<AdminUserDto> Users, int Total)> GetUsersPagedAsync(string? search, int page, int pageSize);
        Task<bool> DisableUserAsync(int userId);
        Task<bool> EnableUserAsync(int userId);
        Task<IEnumerable<RoleDto>> GetAllRolesAsync();
        Task SetUserRolesAsync(int userId, IEnumerable<int> roleIds, int adminUserId);
    }
}
