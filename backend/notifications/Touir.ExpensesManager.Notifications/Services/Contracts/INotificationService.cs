using Touir.ExpensesManager.Notifications.Messaging.Messages;
using Touir.ExpensesManager.Notifications.Models;

namespace Touir.ExpensesManager.Notifications.Services.Contracts
{
    public interface INotificationService
    {
        Task HandleFamilyMemberRemovedAsync(FamilyEventMessage message);
        Task<IEnumerable<Notification>> GetNotificationsAsync(int userId, int page, int pageSize);
        Task<int> GetUnreadCountAsync(int userId);
        Task MarkAsReadAsync(long notificationId, int userId);
        Task MarkAllAsReadAsync(int userId);
    }
}
