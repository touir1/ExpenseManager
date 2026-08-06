using Touir.ExpensesManager.Expenses.Controllers.DTO;

namespace Touir.ExpensesManager.Expenses.Services.Contracts
{
    public interface IRecurringExpenseService
    {
        Task<IEnumerable<RecurringExpenseDto>> GetUpcomingAsync(int userId, int take);
    }
}
