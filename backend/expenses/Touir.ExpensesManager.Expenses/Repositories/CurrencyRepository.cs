using Microsoft.EntityFrameworkCore;
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
    }
}
