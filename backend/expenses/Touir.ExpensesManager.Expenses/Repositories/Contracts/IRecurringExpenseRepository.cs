using Touir.ExpensesManager.Expenses.Models;

namespace Touir.ExpensesManager.Expenses.Repositories.Contracts
{
    public interface IRecurringExpenseRepository
    {
        Task<IEnumerable<RecurringExpense>> GetUpcomingAsync(int userId, int take);
    }
}
