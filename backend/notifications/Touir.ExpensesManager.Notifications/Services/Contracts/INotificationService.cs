using Touir.ExpensesManager.Notifications.Messaging.Messages;
using Touir.ExpensesManager.Notifications.Models;

namespace Touir.ExpensesManager.Notifications.Services.Contracts
{
    public interface INotificationService
    {
        Task HandleFamilyMemberRemovedAsync(FamilyEventMessage message);
        Task HandleFamilyInvitationAsync(FamilyInvitationEventMessage message);
        Task HandleFamilyInvitationAcceptedAsync(FamilyInvitationAcceptedEventMessage message);
        Task HandleFamilyMemberJoinedAsync(FamilyMemberJoinedEventMessage message);
        Task HandleFamilyExpenseAddedAsync(FamilyExpenseEventMessage message);
        Task HandleFamilyExpenseDeletedAsync(FamilyExpenseEventMessage message);
        Task HandleImportCompletedAsync(ImportCompletedEventMessage message);
        Task HandleRateConflictAsync(RateConflictEventMessage message);
        Task HandleEmailVerificationAsync(UserNotificationEventMessage message);
        Task HandlePasswordResetAsync(UserNotificationEventMessage message);
        Task HandlePasswordChangedAsync(UserNotificationEventMessage message);
        Task<IEnumerable<Notification>> GetNotificationsAsync(int userId, int page, int pageSize);
        Task<int> GetUnreadCountAsync(int userId);
        Task MarkAsReadAsync(long notificationId, int userId);
        Task MarkAllAsReadAsync(int userId);
    }
}
