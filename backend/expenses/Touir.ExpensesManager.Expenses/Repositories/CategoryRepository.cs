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
                .Where(c => c.ParentCategoryId == null && !c.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetAllWithArchivedAsync()
        {
            return await _dbContext.Categories
                .Include(c => c.Children)
                .Where(c => c.ParentCategoryId == null)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> ExistsWithNameAsync(string name, int? parentCategoryId, int? excludeId = null)
            => await _dbContext.Categories.AnyAsync(c =>
                c.Name.ToLower() == name.ToLower()
                && c.ParentCategoryId == parentCategoryId
                && !c.IsDeleted
                && (excludeId == null || c.Id != excludeId));

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _dbContext.Categories
                .Include(c => c.Children)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Category> AddAsync(Category category)
        {
            _dbContext.Categories.Add(category);
            await _dbContext.SaveChangesAsync();
            return category;
        }

        public async Task UpdateAsync(Category category)
        {
            _dbContext.Categories.Update(category);
            await _dbContext.SaveChangesAsync();
        }

        public async Task ArchiveAsync(int id)
        {
            var category = await _dbContext.Categories.FindAsync(id);
            if (category is null) return;
            category.IsDeleted = true;
            category.DeletedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }

        public async Task UnarchiveAsync(int id)
        {
            var category = await _dbContext.Categories.FindAsync(id);
            if (category is null) return;
            category.IsDeleted = false;
            category.DeletedAt = null;
            await _dbContext.SaveChangesAsync();
        }
    }
}
