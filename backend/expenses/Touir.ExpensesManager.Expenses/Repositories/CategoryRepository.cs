using Microsoft.EntityFrameworkCore;
using Touir.ExpensesManager.Expenses.Infrastructure;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;

namespace Touir.ExpensesManager.Expenses.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ExpensesDbContext _dbContext;

        public CategoryRepository(ExpensesDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Category>> GetAllActiveAsync()
        {
            return await _dbContext.Categories
                .Include(c => c.Children)
                .Where(c => c.ParentCategoryId == null && !c.IsArchived)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
