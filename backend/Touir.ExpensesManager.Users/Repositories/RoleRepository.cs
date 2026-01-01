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
    }
}
