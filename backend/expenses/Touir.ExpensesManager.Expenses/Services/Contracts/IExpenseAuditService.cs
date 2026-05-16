using Touir.ExpensesManager.Expenses.Models;

namespace Touir.ExpensesManager.Expenses.Services.Contracts
{
    public interface IExpenseAuditService
    {
        Task WriteAddAuditAsync(Expense expense, int performedById, int performedFromId, string tags = "");
        Task WriteUpdateAuditAsync(Expense before, Expense after, int performedById, int performedFromId, string beforeTags = "", string afterTags = "");
        Task WriteDeleteAuditAsync(Expense expense, int performedById, int performedFromId, string tags = "");
    }
}
