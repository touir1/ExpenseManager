using Touir.ExpensesManager.Expenses.Models;

namespace Touir.ExpensesManager.Expenses.Repositories.Contracts
{
    public interface IRecurringExpenseRepository
    {
        Task<IEnumerable<RecurringExpense>> GetUpcomingAsync(int userId, int take);
        Task<IEnumerable<RecurringExpense>> GetPagedAsync(int userId, bool includeInactive);
        Task<RecurringExpense?> GetByIdAsync(int id, int userId);
        Task<RecurringExpense> AddAsync(RecurringExpense recurringExpense);
        Task UpdateAsync(RecurringExpense recurringExpense);
        Task<bool> SoftDeleteAsync(int id, int userId);
        Task<IEnumerable<RecurringExpense>> GetDueForGenerationAsync(DateOnly asOfDate);
    }
}
