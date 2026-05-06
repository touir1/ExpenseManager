using Touir.ExpensesManager.Users.Models;

namespace Touir.ExpensesManager.Users.Repositories.Contracts
{
    public interface IOutboxRepository
    {
        Task EnqueueAsync(OutboxEvent outboxEvent);
        Task<IReadOnlyList<OutboxEvent>> GetPendingAsync(int maxRetries);
        Task MarkPublishedAsync(long id);
        Task MarkFailedAsync(long id, string error);
        Task<int> RequeueAsync(string? eventType, DateTime? from, bool forceAll = false);
    }
}
