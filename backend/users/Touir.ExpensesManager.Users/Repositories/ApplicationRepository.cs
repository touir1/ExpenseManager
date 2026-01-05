using Microsoft.EntityFrameworkCore;
using Touir.ExpensesManager.Users.Infrastructure;
using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Repositories.Contracts;

namespace Touir.ExpensesManager.Users.Repositories
{
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly UsersAppDbContext _context;
        public ApplicationRepository(UsersAppDbContext context)
        {
            _context = context;
        }

        public async Task<Application> GetApplicationByCodeAsync(string applicationCode)
        {
            return await _context.Applications
                .Where(a => a.Code == applicationCode)
                .FirstOrDefaultAsync();
        }
    }
}
