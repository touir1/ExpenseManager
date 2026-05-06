using Touir.ExpensesManager.Expenses.Models;

namespace Touir.ExpensesManager.Expenses.Repositories.Contracts
{
    public interface IInboxRepository
    {
        Task<bool> ExistsAsync(string messageId);
        Task AddAsync(InboxEvent inboxEvent);
    }
}
