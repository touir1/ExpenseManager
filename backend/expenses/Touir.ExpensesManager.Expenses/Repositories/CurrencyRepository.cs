using Microsoft.EntityFrameworkCore;
using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Infrastructure;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;

namespace Touir.ExpensesManager.Expenses.Repositories
{
    public class CurrencyRepository : ICurrencyRepository
    {
        private readonly ExpensesDbContext _dbContext;

        public CurrencyRepository(ExpensesDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Currency>> GetAllAsync()
        {
            return await _dbContext.Currencies
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Currency?> GetByIdAsync(int id)
            => await _dbContext.Currencies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

        public async Task<bool> ExistsByCodeAsync(string code)
            => await _dbContext.Currencies.AnyAsync(c => c.Code == code);

        public async Task<Currency> AddAsync(Currency currency)
        {
            _dbContext.Currencies.Add(currency);
            await _dbContext.SaveChangesAsync();
            return currency;
        }

        public async Task<Currency> UpdateAsync(Currency currency)
        {
            _dbContext.Currencies.Update(currency);
            await _dbContext.SaveChangesAsync();
            return currency;
        }

        public async Task DeleteAsync(int id)
        {
            var currency = await _dbContext.Currencies.FindAsync(id);
            if (currency is not null)
            {
                _dbContext.Currencies.Remove(currency);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<CurrencyDefaultRateDto>> GetDefaultsForSourceAsync(int sourceCurrencyId)
        {
            var defaults = await _dbContext.CurrencyPairDefaults
                .AsNoTracking()
                .Include(d => d.DestinationCurrency)
                .Where(d => d.SourceCurrencyId == sourceCurrencyId)
                .ToListAsync();

            var latestAutoRates = await _dbContext.CurrencyDailyRates
                .AsNoTracking()
                .Where(r => r.SourceCurrencyId == sourceCurrencyId && r.RateSourceId == 1)
                .GroupBy(r => r.DestinationCurrencyId)
                .Select(g => g.OrderByDescending(r => r.Date).First())
                .ToListAsync();

            var autoRateByDest = latestAutoRates.ToDictionary(r => r.DestinationCurrencyId);

            return defaults.Select(d =>
            {
                autoRateByDest.TryGetValue(d.DestinationCurrencyId, out var auto);
                return new CurrencyDefaultRateDto
                {
                    DestinationCurrencyId = d.DestinationCurrencyId,
                    DestinationCode = d.DestinationCurrency.Code,
                    DefaultRate = d.Rate,
                    LastAutoRate = auto?.Rate,
                    LastAutoRateDate = auto?.Date.ToString("yyyy-MM-dd")
                };
            });
        }
    }
}
