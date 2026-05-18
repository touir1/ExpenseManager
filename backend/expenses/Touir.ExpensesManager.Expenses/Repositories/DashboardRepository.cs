using Microsoft.EntityFrameworkCore;
using Touir.ExpensesManager.Expenses.Infrastructure;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;

namespace Touir.ExpensesManager.Expenses.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly ExpensesDbContext _db;

        public DashboardRepository(ExpensesDbContext db)
        {
            _db = db;
        }

        private IQueryable<Expense> BaseQuery(int userId, int? familyId)
        {
            if (familyId.HasValue)
                return _db.Expenses
                    .Where(e => !e.IsDeleted &&
                                _db.ExpenseFamilyAttributions.Any(efa => efa.ExpenseId == e.Id && efa.FamilyId == familyId.Value));

            return _db.Expenses
                .Where(e => !e.IsDeleted && e.UserId == userId);
        }

        public async Task<IEnumerable<CurrencyTotalRow>> GetTotalsAsync(
            int userId, int? familyId, DateOnly dateFrom, DateOnly dateTo)
        {
            var rows = await BaseQuery(userId, familyId)
                .Where(e => e.Date >= dateFrom && e.Date <= dateTo)
                .Select(e => new { e.CurrencyId, e.Amount })
                .AsNoTracking()
                .ToListAsync();

            return rows
                .GroupBy(e => e.CurrencyId)
                .Select(g => new CurrencyTotalRow(g.Key, g.Sum(e => e.Amount), g.Count()));
        }

        public async Task<IEnumerable<CategoryTotalRow>> GetCategoryTotalsAsync(
            int userId, int? familyId, DateOnly dateFrom, DateOnly dateTo)
        {
            var rows = await BaseQuery(userId, familyId)
                .Where(e => e.Date >= dateFrom && e.Date <= dateTo)
                .Select(e => new { e.CategoryId, e.SubcategoryId, e.CurrencyId, e.Amount })
                .AsNoTracking()
                .ToListAsync();

            return rows
                .GroupBy(e => new { e.CategoryId, e.SubcategoryId, e.CurrencyId })
                .Select(g => new CategoryTotalRow(g.Key.CategoryId, g.Key.SubcategoryId, g.Key.CurrencyId, g.Sum(e => e.Amount)));
        }

        public async Task<IEnumerable<MonthlyTotalRow>> GetMonthlyTotalsAsync(
            int userId, int? familyId, DateOnly dateFrom, DateOnly dateTo)
        {
            var rows = await BaseQuery(userId, familyId)
                .Where(e => e.Date >= dateFrom && e.Date <= dateTo)
                .Select(e => new { e.Date, e.CurrencyId, e.Amount })
                .AsNoTracking()
                .ToListAsync();

            return rows
                .GroupBy(e => new { e.Date.Year, e.Date.Month, e.CurrencyId })
                .Select(g => new MonthlyTotalRow(g.Key.Year, g.Key.Month, g.Key.CurrencyId, g.Sum(e => e.Amount)));
        }

        public async Task<IEnumerable<MonthlyCategoryTotalRow>> GetMonthlyCategoryTotalsAsync(
            int userId, int? familyId, DateOnly dateFrom, DateOnly dateTo)
        {
            var rows = await BaseQuery(userId, familyId)
                .Where(e => e.Date >= dateFrom && e.Date <= dateTo)
                .Select(e => new { e.Date, e.CategoryId, e.CurrencyId, e.Amount })
                .AsNoTracking()
                .ToListAsync();

            return rows
                .GroupBy(e => new { e.Date.Year, e.Date.Month, e.CategoryId, e.CurrencyId })
                .Select(g => new MonthlyCategoryTotalRow(g.Key.Year, g.Key.Month, g.Key.CategoryId, g.Key.CurrencyId, g.Sum(e => e.Amount)));
        }

        public async Task<IEnumerable<YearlyTotalRow>> GetYearlyTotalsForMonthAsync(
            int userId, int? familyId, int month)
        {
            var rows = await BaseQuery(userId, familyId)
                .Select(e => new { e.Date, e.CurrencyId, e.Amount })
                .AsNoTracking()
                .ToListAsync();

            return rows
                .Where(e => e.Date.Month == month)
                .GroupBy(e => new { e.Date.Year, e.CurrencyId })
                .Select(g => new YearlyTotalRow(g.Key.Year, g.Key.CurrencyId, g.Sum(e => e.Amount)));
        }
    }
}
