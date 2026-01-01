using Microsoft.EntityFrameworkCore;
using Touir.ExpensesManager.Users.Infrastructure;
using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Services.Contracts;

namespace Touir.ExpensesManager.Users.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly UsersAppDbContext _dbContext;
        public ApplicationService(UsersAppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Application> GetApplicationByCodeAsync(string applicationCode)
        {
            var application = await _dbContext.Applications
                .FirstOrDefaultAsync(app => app.Code == applicationCode);
            if (application == null)
            {
                return null;
            }
            return application;
        }
    }
}
