using Microsoft.EntityFrameworkCore;
using Touir.ExpensesManager.Expenses.Infrastructure;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;

namespace Touir.ExpensesManager.Expenses.Repositories
{
    public class RecurringExpenseRepository : IRecurringExpenseRepository
    {
        private readonly ExpensesDbContext _db;

        public RecurringExpenseRepository(ExpensesDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<RecurringExpense>> GetUpcomingAsync(int userId, int take)
        {
            return await _db.RecurringExpenses
                .Where(r => r.UserId == userId && r.IsActive && !r.IsDeleted)
                .OrderBy(r => r.NextDueDate)
                .Take(take)
                .Include(r => r.Currency)
                .Include(r => r.Category)
                .Include(r => r.Subcategory)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<RecurringExpense>> GetPagedAsync(int userId, bool includeInactive)
        {
            var query = _db.RecurringExpenses
                .Where(r => r.UserId == userId && !r.IsDeleted);

            if (!includeInactive)
                query = query.Where(r => r.IsActive);

            return await query
                .OrderBy(r => r.NextDueDate)
                .Include(r => r.Currency)
                .Include(r => r.Category)
                .Include(r => r.Subcategory)
                .Include(r => r.Family)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<RecurringExpense?> GetByIdAsync(int id, int userId)
        {
            return await _db.RecurringExpenses
                .Include(r => r.Currency)
                .Include(r => r.Category)
                .Include(r => r.Subcategory)
                .Include(r => r.Family)
                .Where(r => r.Id == id && r.UserId == userId && !r.IsDeleted)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<RecurringExpense> AddAsync(RecurringExpense recurringExpense)
        {
            _db.RecurringExpenses.Add(recurringExpense);
            await _db.SaveChangesAsync();
            return recurringExpense;
        }

        public async Task UpdateAsync(RecurringExpense recurringExpense)
        {
            _db.RecurringExpenses.Update(recurringExpense);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> SoftDeleteAsync(int id, int userId)
        {
            var existing = await _db.RecurringExpenses
                .Where(r => r.Id == id && r.UserId == userId && !r.IsDeleted)
                .FirstOrDefaultAsync();

            if (existing is null)
                return false;

            existing.IsDeleted = true;
            existing.DeletedAt = DateTime.UtcNow;
            _db.RecurringExpenses.Update(existing);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<RecurringExpense>> GetDueForGenerationAsync(DateOnly asOfDate)
        {
            return await _db.RecurringExpenses
                .Where(r => r.IsActive && !r.IsDeleted
                    && r.NextDueDate <= asOfDate
                    && (r.LastGeneratedDate == null || r.LastGeneratedDate < r.NextDueDate))
                .ToListAsync();
        }
    }
}
