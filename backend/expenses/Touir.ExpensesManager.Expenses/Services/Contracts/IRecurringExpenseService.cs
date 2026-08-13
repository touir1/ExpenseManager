using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Requests;

namespace Touir.ExpensesManager.Expenses.Services.Contracts
{
    public interface IRecurringExpenseService
    {
        Task<IEnumerable<RecurringExpenseDto>> GetUpcomingAsync(int userId, int take);
        Task<IEnumerable<RecurringExpenseDto>> GetAllAsync(int userId, bool includeInactive);
        Task<RecurringExpenseDto?> GetByIdAsync(int id, int userId);
        Task<RecurringExpenseDto> CreateAsync(CreateRecurringExpenseRequest request, int userId);
        Task<RecurringExpenseDto?> UpdateAsync(int id, UpdateRecurringExpenseRequest request, int userId);
        Task<bool> DeleteAsync(int id, int userId);
        Task<ExpenseDto?> ConfirmAsync(int id, int userId);
        Task GenerateDueAsync(DateOnly asOfDate, CancellationToken cancellationToken = default);
    }
}
