using Touir.ExpensesManager.Expenses.Models.External;
using Microsoft.EntityFrameworkCore;
using Touir.ExpensesManager.Expenses.Infrastructure;
using Touir.ExpensesManager.Expenses.Repositories.External.Contracts;

namespace Touir.ExpensesManager.Expenses.Repositories.External
{
    public class UserRepository : IUserRepository
    {
        private readonly ExpensesDbContext _dbContext;

        public UserRepository(ExpensesDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task DeleteUserAsync(User user)
        {
            var existing = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            if (existing != null)
            {
                existing.IsDeleted = true;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<IEnumerable<User>> GetUsersByFamilyIdAsync(int familyId)
        {
            return await _dbContext.Users
                .Where(u => u.FamilyId == familyId)
                .ToListAsync();
        }

        public async Task SaveOrUpdateUserAsync(User user)
        {
            var existing = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            if (existing == null)
            {
                user.IsDeleted = false;
                await _dbContext.Users.AddAsync(user);
            }
            else
            {
                existing.FirstName = user.FirstName;
                existing.LastName = user.LastName;
                existing.Email = user.Email;
                existing.FamilyId = user.FamilyId;
            }
            await _dbContext.SaveChangesAsync();
        }
    }
}
