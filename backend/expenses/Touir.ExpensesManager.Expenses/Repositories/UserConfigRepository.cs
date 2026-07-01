using Microsoft.EntityFrameworkCore;
using System.Text.Json;
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

        public async Task<UserConfig> UpsertAsync(int userId, int? defaultCurrencyId, int? defaultCategoryId)
        {
            var existing = await _dbContext.UserConfigs
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (existing is null)
            {
                existing = new UserConfig
                {
                    UserId = userId,
                    DefaultCurrencyId = defaultCurrencyId,
                    DefaultCategoryId = defaultCategoryId,
                };
                _dbContext.UserConfigs.Add(existing);
            }
            else
            {
                existing.DefaultCurrencyId = defaultCurrencyId;
                existing.DefaultCategoryId = defaultCategoryId;
            }

            await _dbContext.SaveChangesAsync();

            await _dbContext.Entry(existing)
                .Reference(c => c.DefaultCurrency)
                .LoadAsync();

            return existing;
        }

        public async Task<Dictionary<string, string>?> GetDefaultCsvColumnMappingAsync(int userId)
        {
            var json = await _dbContext.UserConfigs
                .Where(c => c.UserId == userId)
                .Select(c => c.DefaultCsvColumnMappingJson)
                .FirstOrDefaultAsync();

            return json is null ? null : JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        }

        public async Task<UserConfig> UpsertCsvColumnMappingAsync(int userId, Dictionary<string, string>? mapping)
        {
            var existing = await _dbContext.UserConfigs
                .FirstOrDefaultAsync(c => c.UserId == userId);

            var json = mapping is null ? null : JsonSerializer.Serialize(mapping);

            if (existing is null)
            {
                existing = new UserConfig
                {
                    UserId = userId,
                    DefaultCsvColumnMappingJson = json,
                };
                _dbContext.UserConfigs.Add(existing);
            }
            else
            {
                existing.DefaultCsvColumnMappingJson = json;
            }

            await _dbContext.SaveChangesAsync();

            await _dbContext.Entry(existing)
                .Reference(c => c.DefaultCurrency)
                .LoadAsync();

            return existing;
        }
    }
}
