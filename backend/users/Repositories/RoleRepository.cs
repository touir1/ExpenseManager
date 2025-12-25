using com.touir.expenses.Users.Infrastructure;
using com.touir.expenses.Users.Models;
using com.touir.expenses.Users.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace com.touir.expenses.Users.Repositories
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
