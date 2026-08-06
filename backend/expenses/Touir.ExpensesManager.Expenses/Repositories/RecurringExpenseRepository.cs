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
    }
}
