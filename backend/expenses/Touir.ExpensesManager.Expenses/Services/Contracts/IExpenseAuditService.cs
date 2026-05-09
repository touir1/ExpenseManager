using Touir.ExpensesManager.Expenses.Models;

namespace Touir.ExpensesManager.Expenses.Services.Contracts
{
    public interface IExpenseAuditService
    {
        Task WriteAddAuditAsync(Expense expense, int performedById, int performedFromId);
        Task WriteUpdateAuditAsync(Expense before, Expense after, int performedById, int performedFromId);
        Task WriteDeleteAuditAsync(Expense expense, int performedById, int performedFromId);
    }
}
