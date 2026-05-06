using Microsoft.EntityFrameworkCore;
using Touir.ExpensesManager.Expenses.Infrastructure;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;

namespace Touir.ExpensesManager.Expenses.Repositories
{
    public class InboxRepository : IInboxRepository
    {
        private readonly ExpensesDbContext _context;

        public InboxRepository(ExpensesDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(string messageId)
        {
            return await _context.InboxEvents.AnyAsync(e => e.MessageId == messageId);
        }

        public async Task AddAsync(InboxEvent inboxEvent)
        {
            await _context.InboxEvents.AddAsync(inboxEvent);
            await _context.SaveChangesAsync();
        }
    }
}
