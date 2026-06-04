using Microsoft.EntityFrameworkCore;
using Touir.ExpensesManager.Expenses.Infrastructure;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;

namespace Touir.ExpensesManager.Expenses.Repositories
{
    public class ExpensesOutboxRepository : IExpensesOutboxRepository
    {
        private readonly ExpensesDbContext _db;

        public ExpensesOutboxRepository(ExpensesDbContext db)
        {
            _db = db;
        }

        public async Task EnqueueAsync(OutboxEvent outboxEvent)
        {
            await _db.OutboxEvents.AddAsync(outboxEvent);
            await _db.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<OutboxEvent>> GetPendingAsync(int maxRetries)
            => await _db.OutboxEvents
                .Where(e => e.PublishedAt == null && e.RetryCount < maxRetries)
                .OrderBy(e => e.CreatedAt)
                .ToListAsync();

        public async Task MarkPublishedAsync(long id)
        {
            var evt = await _db.OutboxEvents.FindAsync(id);
            if (evt is null) return;
            evt.PublishedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task MarkFailedAsync(long id, string error)
        {
            var evt = await _db.OutboxEvents.FindAsync(id);
            if (evt is null) return;
            evt.RetryCount++;
            evt.LastError = error.Length > 2000 ? error[..2000] : error;
            await _db.SaveChangesAsync();
        }
    }
}
