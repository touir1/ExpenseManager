using Touir.ExpensesManager.Expenses.Models;

namespace Touir.ExpensesManager.Expenses.Repositories.Contracts
{
    public interface IExpensesOutboxRepository
    {
        Task EnqueueAsync(OutboxEvent outboxEvent);
        Task<IReadOnlyList<OutboxEvent>> GetPendingAsync(int maxRetries);
        Task MarkPublishedAsync(long id);
        Task MarkFailedAsync(long id, string error);
    }
}
