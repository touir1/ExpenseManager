using Microsoft.EntityFrameworkCore;
using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Infrastructure;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;

namespace Touir.ExpensesManager.Expenses.Repositories
{
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly ExpensesDbContext _dbContext;

        public ExpenseRepository(ExpensesDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Expense> AddAsync(Expense expense)
        {
            _dbContext.Expenses.Add(expense);
            await _dbContext.SaveChangesAsync();
            return expense;
        }

        public async Task UpdateAsync(Expense expense)
        {
            _dbContext.Expenses.Update(expense);
            await _dbContext.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(Expense expense)
        {
            expense.IsDeleted = true;
            expense.DeletedAt = DateTime.UtcNow;
            _dbContext.Expenses.Update(expense);
            await _dbContext.SaveChangesAsync();
        }

        public async Task ClearExpenseTagsAsync(long expenseId)
        {
            var tags = await _dbContext.ExpenseTags
                .Where(et => et.ExpenseId == expenseId)
                .ToListAsync();
            _dbContext.ExpenseTags.RemoveRange(tags);
            await _dbContext.SaveChangesAsync();
        }

        public async Task AddExpenseTagsAsync(IEnumerable<ExpenseTag> expenseTags)
        {
            _dbContext.ExpenseTags.AddRange(expenseTags);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Expense?> GetByIdAsync(long id, int userId)
        {
            return await _dbContext.Expenses
                .Include(e => e.Currency)
                .Include(e => e.Category)
                .Include(e => e.Subcategory)
                .Include(e => e.ModifiedFrom)
                .Include(e => e.ExpenseTags).ThenInclude(et => et.Tag)
                .Where(e => e.Id == id && e.UserId == userId && !e.IsDeleted)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<(IEnumerable<Expense> Items, int TotalCount)> GetPagedAsync(ExpenseFilterDto filter, int userId)
        {
            var query = _dbContext.Expenses
                .Include(e => e.Currency)
                .Include(e => e.Category)
                .Include(e => e.Subcategory)
                .Include(e => e.ModifiedFrom)
                .Include(e => e.ExpenseTags).ThenInclude(et => et.Tag)
                .Where(e => e.UserId == userId && !e.IsDeleted);

            if (filter.DateFrom.HasValue)
                query = query.Where(e => e.Date >= filter.DateFrom.Value);
            if (filter.DateTo.HasValue)
                query = query.Where(e => e.Date <= filter.DateTo.Value);
            if (filter.CategoryId.HasValue)
                query = query.Where(e => e.CategoryId == filter.CategoryId.Value);
            if (filter.SubcategoryId.HasValue)
                query = query.Where(e => e.SubcategoryId == filter.SubcategoryId.Value);
            if (filter.CurrencyId.HasValue)
                query = query.Where(e => e.CurrencyId == filter.CurrencyId.Value);
            if (filter.AmountMin.HasValue)
                query = query.Where(e => e.Amount >= filter.AmountMin.Value);
            if (filter.AmountMax.HasValue)
                query = query.Where(e => e.Amount <= filter.AmountMax.Value);
            if (!string.IsNullOrWhiteSpace(filter.Description))
                query = query.Where(e => e.Description != null && e.Description.Contains(filter.Description));
            if (filter.TagIds is { Length: > 0 })
                query = query.Where(e => e.ExpenseTags.Any(et => filter.TagIds.Contains(et.TagId)));

            var totalCount = await query.CountAsync();

            var pageSize = Math.Max(1, filter.PageSize);
            var page = Math.Max(1, filter.Page);

            var items = await query
                .OrderByDescending(e => e.Date)
                .ThenByDescending(e => e.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
