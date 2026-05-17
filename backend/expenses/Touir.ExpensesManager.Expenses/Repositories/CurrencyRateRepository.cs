using Microsoft.EntityFrameworkCore;
using Touir.ExpensesManager.Expenses.Infrastructure;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;

namespace Touir.ExpensesManager.Expenses.Repositories
{
    public class CurrencyRateRepository : ICurrencyRateRepository
    {
        private readonly ExpensesDbContext _db;

        public CurrencyRateRepository(ExpensesDbContext db)
        {
            _db = db;
        }

        public async Task<CurrencyDailyRate?> GetExactAsync(int sourceId, int destId, DateOnly date)
        {
            return await _db.CurrencyDailyRates
                .AsNoTracking()
                .Include(r => r.SourceCurrency)
                .Include(r => r.DestinationCurrency)
                .Include(r => r.RateSource)
                .FirstOrDefaultAsync(r =>
                    r.SourceCurrencyId == sourceId &&
                    r.DestinationCurrencyId == destId &&
                    r.Date == date);
        }

        public async Task<CurrencyDailyRate?> GetMostRecentBeforeAsync(int sourceId, int destId, DateOnly date)
        {
            return await _db.CurrencyDailyRates
                .AsNoTracking()
                .Include(r => r.SourceCurrency)
                .Include(r => r.DestinationCurrency)
                .Include(r => r.RateSource)
                .Where(r =>
                    r.SourceCurrencyId == sourceId &&
                    r.DestinationCurrencyId == destId &&
                    r.Date < date)
                .OrderByDescending(r => r.Date)
                .FirstOrDefaultAsync();
        }

        public async Task<CurrencyPairDefault?> GetDefaultAsync(int sourceId, int destId)
        {
            return await _db.CurrencyPairDefaults
                .AsNoTracking()
                .Include(r => r.SourceCurrency)
                .Include(r => r.DestinationCurrency)
                .FirstOrDefaultAsync(r =>
                    r.SourceCurrencyId == sourceId &&
                    r.DestinationCurrencyId == destId);
        }

        public async Task<IEnumerable<CurrencyDailyRate>> GetHistoryAsync(int sourceId, int destId)
        {
            return await _db.CurrencyDailyRates
                .AsNoTracking()
                .Include(r => r.RateSource)
                .Where(r =>
                    r.SourceCurrencyId == sourceId &&
                    r.DestinationCurrencyId == destId)
                .OrderByDescending(r => r.Date)
                .ToListAsync();
        }

        public async Task AddRateAsync(CurrencyDailyRate rate)
        {
            _db.CurrencyDailyRates.Add(rate);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateRateAsync(CurrencyDailyRate rate)
        {
            _db.CurrencyDailyRates.Update(rate);
            await _db.SaveChangesAsync();
        }

        public async Task AddConflictAsync(CurrencyRateConflict conflict)
        {
            _db.CurrencyRateConflicts.Add(conflict);
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<CurrencyRateConflict>> GetPendingConflictsAsync()
        {
            return await _db.CurrencyRateConflicts
                .AsNoTracking()
                .Include(c => c.SourceCurrency)
                .Include(c => c.DestinationCurrency)
                .Include(c => c.Status)
                .Where(c => c.StatusId == 1) // Pending
                .OrderByDescending(c => c.Date)
                .ToListAsync();
        }

        public async Task<CurrencyRateConflict?> GetConflictByIdAsync(int id)
        {
            return await _db.CurrencyRateConflicts
                .Include(c => c.SourceCurrency)
                .Include(c => c.DestinationCurrency)
                .Include(c => c.Status)
                .Include(c => c.Resolution)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task UpdateConflictAsync(CurrencyRateConflict conflict)
        {
            _db.CurrencyRateConflicts.Update(conflict);
            await _db.SaveChangesAsync();
        }

        public async Task SetDefaultAsync(CurrencyPairDefault pair)
        {
            var existing = await _db.CurrencyPairDefaults
                .FirstOrDefaultAsync(p =>
                    p.SourceCurrencyId == pair.SourceCurrencyId &&
                    p.DestinationCurrencyId == pair.DestinationCurrencyId);

            if (existing is null)
                _db.CurrencyPairDefaults.Add(pair);
            else
                existing.Rate = pair.Rate;

            await _db.SaveChangesAsync();
        }
    }
}
