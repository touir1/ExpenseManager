using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using Touir.ExpensesManager.Notifications.Hubs;
using Touir.ExpensesManager.Notifications.Infrastructure;
using Touir.ExpensesManager.Notifications.Infrastructure.Contracts;
using Touir.ExpensesManager.Notifications.Messaging.Messages;
using Touir.ExpensesManager.Notifications.Models;
using Touir.ExpensesManager.Notifications.Repositories.Contracts;
using Touir.ExpensesManager.Notifications.Services.Contracts;

namespace Touir.ExpensesManager.Notifications.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notifRepo;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IEmailHelper _emailHelper;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            INotificationRepository notifRepo,
            IHubContext<NotificationHub> hubContext,
            IEmailHelper emailHelper,
            ILogger<NotificationService> logger)
        {
            _notifRepo = notifRepo;
            _hubContext = hubContext;
            _emailHelper = emailHelper;
            _logger = logger;
        }

        public async Task HandleFamilyMemberRemovedAsync(FamilyEventMessage message)
        {
            var payload = JsonSerializer.Serialize(new
            {
                type = NotificationType.FamilyMemberRemoved,
                familyId = message.FamilyId,
                familyName = message.FamilyName,
                removedByUserId = message.RemovedByUserId,
                removedByName = message.RemovedByName,
                expenseCount = message.ExpenseCount
            });

            var notification = await _notifRepo.CreateAsync(new Notification
            {
                UserId = message.TargetUserId,
                Type = NotificationType.FamilyMemberRemoved,
                Payload = payload,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });

            try
            {
                await _hubContext.Clients
                    .Group(message.TargetUserId.ToString())
                    .SendAsync("notification", new
                    {
                        id = notification.Id,
                        type = notification.Type,
                        payload = JsonSerializer.Deserialize<JsonElement>(payload),
                        createdAt = notification.CreatedAt,
                        isRead = false
                    });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[SignalR] push failed for user {UserId}", message.TargetUserId);
            }

            try
            {
                var emailHtml = _emailHelper.GetEmailTemplate(
                    EmailHtmlTemplate.FamilyMemberRemoved.Key,
                    new Dictionary<string, string>
                    {
                        { EmailHtmlTemplate.FamilyMemberRemoved.Variables.FamilyName, message.FamilyName },
                        { EmailHtmlTemplate.FamilyMemberRemoved.Variables.RemovedByName, message.RemovedByName },
                        { EmailHtmlTemplate.FamilyMemberRemoved.Variables.ExpenseCount, message.ExpenseCount.ToString() }
                    });
                _emailHelper.SendEmail(
                    recipientTo: message.TargetEmail,
                    emailSubject: "[Expenses Manager] You have been removed from a family",
                    isHTML: true,
                    emailBody: emailHtml);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Email] send failed for {Email}", message.TargetEmail);
            }
        }

        public Task<IEnumerable<Notification>> GetNotificationsAsync(int userId, int page, int pageSize)
            => _notifRepo.GetByUserAsync(userId, page, pageSize);

        public Task<int> GetUnreadCountAsync(int userId)
            => _notifRepo.GetUnreadCountAsync(userId);

        public Task MarkAsReadAsync(long notificationId, int userId)
            => _notifRepo.MarkAsReadAsync(notificationId, userId);

        public Task MarkAllAsReadAsync(int userId)
            => _notifRepo.MarkAllAsReadAsync(userId);
    }
}
