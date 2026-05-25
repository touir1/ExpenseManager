using Microsoft.EntityFrameworkCore;
using Touir.ExpensesManager.Expenses.Infrastructure;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;

namespace Touir.ExpensesManager.Expenses.Repositories
{
    public class UserConfigRepository : IUserConfigRepository
    {
        private readonly ExpensesDbContext _dbContext;

        public UserConfigRepository(ExpensesDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserConfig?> GetByUserIdAsync(int userId)
            => await _dbContext.UserConfigs
                .Include(c => c.DefaultCurrency)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == userId);

        public async Task<UserConfig> UpsertAsync(int userId, int? defaultCurrencyId)
        {
            var existing = await _dbContext.UserConfigs
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (existing is null)
            {
                existing = new UserConfig { UserId = userId, DefaultCurrencyId = defaultCurrencyId };
                _dbContext.UserConfigs.Add(existing);
            }
            else
            {
                existing.DefaultCurrencyId = defaultCurrencyId;
            }

            await _dbContext.SaveChangesAsync();

            await _dbContext.Entry(existing)
                .Reference(c => c.DefaultCurrency)
                .LoadAsync();

            return existing;
        }
    }
}
