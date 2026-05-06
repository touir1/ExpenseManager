using Microsoft.EntityFrameworkCore;
using Touir.ExpensesManager.Users.Infrastructure;
using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Repositories.Contracts;

namespace Touir.ExpensesManager.Users.Repositories
{
    public class OutboxRepository : IOutboxRepository
    {
        private readonly UsersAppDbContext _context;

        public OutboxRepository(UsersAppDbContext context)
        {
            _context = context;
        }

        public async Task EnqueueAsync(OutboxEvent outboxEvent)
        {
            await _context.OutboxEvents.AddAsync(outboxEvent);
            await _context.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<OutboxEvent>> GetPendingAsync(int maxRetries)
        {
            return await _context.OutboxEvents
                .Where(e => e.PublishedAt == null && e.RetryCount < maxRetries)
                .OrderBy(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task MarkPublishedAsync(long id)
        {
            var evt = await _context.OutboxEvents.FindAsync(id);
            if (evt != null)
            {
                evt.PublishedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkFailedAsync(long id, string error)
        {
            var evt = await _context.OutboxEvents.FindAsync(id);
            if (evt != null)
            {
                evt.RetryCount++;
                evt.LastError = error.Length > 2000 ? error[..2000] : error;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> RequeueAsync(string? eventType, DateTime? from, bool forceAll = false)
        {
            var query = _context.OutboxEvents.AsQueryable();

            if (!forceAll)
                query = query.Where(e => e.PublishedAt == null || e.LastError != null);

            if (eventType != null)
                query = query.Where(e => e.EventType == eventType);

            if (from != null)
                query = query.Where(e => e.CreatedAt >= from.Value);

            var events = await query.ToListAsync();
            foreach (var evt in events)
            {
                evt.PublishedAt = null;
                evt.RetryCount = 0;
                evt.LastError = null;
            }

            await _context.SaveChangesAsync();
            return events.Count;
        }
    }
}
