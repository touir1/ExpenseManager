using Touir.ExpensesManager.Notifications.Models;

namespace Touir.ExpensesManager.Notifications.Repositories.Contracts
{
    public interface INotificationRepository
    {
        Task<Notification> CreateAsync(Notification notification);
        Task<IEnumerable<Notification>> GetByUserAsync(int userId, int page, int pageSize);
        Task<int> GetUnreadCountAsync(int userId);
        Task<Notification?> GetByIdAsync(long id, int userId);
        Task MarkAsReadAsync(long id, int userId);
        Task MarkAllAsReadAsync(int userId);
    }
}
