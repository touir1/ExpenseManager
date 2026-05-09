using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Models;

namespace Touir.ExpensesManager.Expenses.Repositories.Contracts
{
    public interface IExpenseRepository
    {
        Task<Expense> AddAsync(Expense expense);
        Task UpdateAsync(Expense expense);
        Task<Expense?> GetByIdAsync(long id, int userId);
        Task<(IEnumerable<Expense> Items, int TotalCount)> GetPagedAsync(ExpenseFilterDto filter, int userId);
        Task SoftDeleteAsync(Expense expense);
    }
}
