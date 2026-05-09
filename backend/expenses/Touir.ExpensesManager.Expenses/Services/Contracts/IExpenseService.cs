using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Requests;

namespace Touir.ExpensesManager.Expenses.Services.Contracts
{
    public interface IExpenseService
    {
        Task<ExpenseDto> AddAsync(CreateExpenseRequest request, int userId, int sourceId);
        Task<ExpenseDto?> UpdateAsync(long id, UpdateExpenseRequest request, int userId, int sourceId);
        Task<bool> DeleteAsync(long id, int userId, int sourceId);
        Task<ExpenseDto?> GetByIdAsync(long id, int userId);
        Task<ExpensePagedResult> GetPagedAsync(ExpenseFilterDto filter, int userId);
    }

    public class ExpensePagedResult
    {
        public IEnumerable<ExpenseDto> Items { get; set; } = [];
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
