using Touir.ExpensesManager.Users.Infrastructure;
using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Touir.ExpensesManager.Users.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly UsersAppDbContext _context;
        public RoleRepository(UsersAppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Role>> GetUserRolesByApplicationCodeAsync(string applicationCode, int userId)
        {
            return await _context.Roles
                .AsNoTracking()
                .Where(w => w.Application.Code == applicationCode && w.UserRoles.Any(ur => ur.UserId == userId))
                .ToListAsync();
        }

        public async Task<Role?> GetDefaultRoleByApplicationIdAsync(int applicationId)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.ApplicationId == applicationId && r.IsDefault);
        }

        public async Task<bool> AssignRoleToUserAsync(int roleId, int userId, int? createdByUserId = null)
        {
            if (roleId == 0 || userId == 0)
                return false;
            var userRole = new UserRole
            {
                RoleId = roleId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                CreatedById = createdByUserId ?? userId
            };
            _context.UserRoles.Add(userRole);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
    }
}
