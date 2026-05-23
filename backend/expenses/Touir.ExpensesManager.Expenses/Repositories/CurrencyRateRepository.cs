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

        public async Task<Dictionary<int, (decimal Rate, int RateSourceId)>> GetExistingOnDateAsync(int sourceId, DateOnly date)
            => await _db.CurrencyDailyRates
                .Where(r => r.SourceCurrencyId == sourceId && r.Date == date)
                .Select(r => new { r.DestinationCurrencyId, r.Rate, r.RateSourceId })
                .ToDictionaryAsync(r => r.DestinationCurrencyId, r => (r.Rate, r.RateSourceId));

        public async Task<Dictionary<(int DestId, DateOnly Date), (decimal Rate, int RateSourceId)>> GetExistingInRangeAsync(int sourceId, DateOnly from, DateOnly to)
            => await _db.CurrencyDailyRates
                .Where(r => r.SourceCurrencyId == sourceId && r.Date >= from && r.Date <= to)
                .Select(r => new { r.DestinationCurrencyId, r.Date, r.Rate, r.RateSourceId })
                .ToDictionaryAsync(r => (r.DestinationCurrencyId, r.Date), r => (r.Rate, r.RateSourceId));

        public async Task<Dictionary<(int SrcId, int DestId, DateOnly Date), (decimal Rate, int RateSourceId)>> GetExistingForPairsAsync(
            IEnumerable<(int srcId, int destId, DateOnly date)> pairs)
        {
            var result = new Dictionary<(int, int, DateOnly), (decimal, int)>();
            var grouped = pairs.GroupBy(p => p.srcId);

            foreach (var group in grouped)
            {
                var srcId = group.Key;
                var dates = group.Select(p => p.date).Distinct().ToList();
                var destIds = group.Select(p => p.destId).Distinct().ToList();

                var rows = await _db.CurrencyDailyRates
                    .Where(r => r.SourceCurrencyId == srcId
                        && dates.Contains(r.Date)
                        && destIds.Contains(r.DestinationCurrencyId))
                    .Select(r => new { r.DestinationCurrencyId, r.Date, r.Rate, r.RateSourceId })
                    .ToListAsync();

                foreach (var row in rows)
                    result[(srcId, row.DestinationCurrencyId, row.Date)] = (row.Rate, row.RateSourceId);
            }

            return result;
        }

        public async Task AddRatesBatchAsync(IEnumerable<CurrencyDailyRate> rates)
        {
            _db.CurrencyDailyRates.AddRange(rates);
            await _db.SaveChangesAsync();
        }

        public async Task AddConflictsBatchAsync(IEnumerable<CurrencyRateConflict> conflicts)
        {
            _db.CurrencyRateConflicts.AddRange(conflicts);
            await _db.SaveChangesAsync();
        }
    }
}
