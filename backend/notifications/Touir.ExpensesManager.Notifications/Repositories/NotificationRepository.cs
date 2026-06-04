using Microsoft.EntityFrameworkCore;
using Touir.ExpensesManager.Notifications.Infrastructure;
using Touir.ExpensesManager.Notifications.Models;
using Touir.ExpensesManager.Notifications.Repositories.Contracts;

namespace Touir.ExpensesManager.Notifications.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationsDbContext _db;

        public NotificationRepository(NotificationsDbContext db)
        {
            _db = db;
        }

        public async Task<Notification> CreateAsync(Notification notification)
        {
            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();
            return notification;
        }

        public async Task<IEnumerable<Notification>> GetByUserAsync(int userId, int page, int pageSize)
            => await _db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

        public async Task<int> GetUnreadCountAsync(int userId)
            => await _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

        public async Task<Notification?> GetByIdAsync(long id, int userId)
            => await _db.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

        public async Task MarkAsReadAsync(long id, int userId)
        {
            var n = await _db.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
            if (n is null || n.IsRead) return;
            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            var unread = await _db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();
            var now = DateTime.UtcNow;
            foreach (var n in unread)
            {
                n.IsRead = true;
                n.ReadAt = now;
            }
            await _db.SaveChangesAsync();
        }
    }
}
