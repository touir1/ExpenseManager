using Microsoft.EntityFrameworkCore;
using Touir.ExpensesManager.Notifications.Infrastructure;
using Touir.ExpensesManager.Notifications.Models;
using Touir.ExpensesManager.Notifications.Repositories.Contracts;

namespace Touir.ExpensesManager.Notifications.Repositories
{
    public class InboxRepository : IInboxRepository
    {
        private readonly NotificationsDbContext _context;

        public InboxRepository(NotificationsDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(string messageId)
            => await _context.InboxEvents.AnyAsync(e => e.MessageId == messageId);

        public async Task AddAsync(InboxEvent inboxEvent)
        {
            await _context.InboxEvents.AddAsync(inboxEvent);
            await _context.SaveChangesAsync();
        }
    }
}
