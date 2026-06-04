using Touir.ExpensesManager.Notifications.Models;

namespace Touir.ExpensesManager.Notifications.Repositories.Contracts
{
    public interface IInboxRepository
    {
        Task<bool> ExistsAsync(string messageId);
        Task AddAsync(InboxEvent inboxEvent);
    }
}
