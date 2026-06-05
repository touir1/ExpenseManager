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

        public Task HandleFamilyInvitationAsync(FamilyInvitationEventMessage message)
        {
            try
            {
                var emailHtml = _emailHelper.GetEmailTemplate(
                    EmailHtmlTemplate.FamilyInvitation.Key,
                    new Dictionary<string, string>
                    {
                        { EmailHtmlTemplate.FamilyInvitation.Variables.InviterName, message.InviterName },
                        { EmailHtmlTemplate.FamilyInvitation.Variables.FamilyName, message.FamilyName },
                        { EmailHtmlTemplate.FamilyInvitation.Variables.InviteLink, message.InviteLink }
                    });
                _emailHelper.SendEmail(
                    recipientTo: message.InviteeEmail,
                    emailSubject: $"[Expenses Manager] You've been invited to join {message.FamilyName}",
                    isHTML: true,
                    emailBody: emailHtml);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Email] family invitation send failed for {Email}", message.InviteeEmail);
            }
            return Task.CompletedTask;
        }

        public async Task HandleFamilyInvitationAcceptedAsync(FamilyInvitationAcceptedEventMessage message)
        {
            var payload = JsonSerializer.Serialize(new
            {
                type = NotificationType.FamilyInvitationAccepted,
                familyId = message.FamilyId,
                familyName = message.FamilyName,
                acceptorName = message.AcceptorName,
                acceptorEmail = message.AcceptorEmail
            });

            Notification? notification = null;
            try
            {
                notification = await _notifRepo.CreateAsync(new Notification
                {
                    UserId = message.HeadUserId,
                    Type = NotificationType.FamilyInvitationAccepted,
                    Payload = payload,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[DB] notification persist failed for invitation accepted, userId {UserId}", message.HeadUserId);
            }

            try
            {
                await _hubContext.Clients
                    .Group(message.HeadUserId.ToString())
                    .SendAsync("notification", new
                    {
                        id = notification?.Id,
                        type = NotificationType.FamilyInvitationAccepted,
                        payload = JsonSerializer.Deserialize<JsonElement>(payload),
                        createdAt = notification?.CreatedAt ?? DateTime.UtcNow,
                        isRead = false
                    });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[SignalR] push failed for userId {UserId}", message.HeadUserId);
            }

            try
            {
                var emailHtml = _emailHelper.GetEmailTemplate(
                    EmailHtmlTemplate.FamilyInvitationAccepted.Key,
                    new Dictionary<string, string>
                    {
                        { EmailHtmlTemplate.FamilyInvitationAccepted.Variables.AcceptorName, message.AcceptorName },
                        { EmailHtmlTemplate.FamilyInvitationAccepted.Variables.FamilyName, message.FamilyName }
                    });
                _emailHelper.SendEmail(
                    recipientTo: message.AcceptorEmail,
                    emailSubject: $"[Expenses Manager] {message.AcceptorName} joined {message.FamilyName}",
                    isHTML: true,
                    emailBody: emailHtml);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Email] invitation accepted email failed for userId {UserId}", message.HeadUserId);
            }
        }

        public async Task HandleFamilyMemberJoinedAsync(FamilyMemberJoinedEventMessage message)
        {
            var payload = JsonSerializer.Serialize(new
            {
                type = NotificationType.FamilyMemberJoined,
                familyId = message.FamilyId,
                familyName = message.FamilyName,
                joinerName = message.JoinerName,
                joinerUserId = message.JoinerUserId
            });

            foreach (var memberId in message.MemberUserIds)
            {
                Notification? notification = null;
                try
                {
                    notification = await _notifRepo.CreateAsync(new Notification
                    {
                        UserId = memberId,
                        Type = NotificationType.FamilyMemberJoined,
                        Payload = payload,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[DB] notification persist failed for member joined, userId {UserId}", memberId);
                }

                try
                {
                    await _hubContext.Clients
                        .Group(memberId.ToString())
                        .SendAsync("notification", new
                        {
                            id = notification?.Id,
                            type = NotificationType.FamilyMemberJoined,
                            payload = JsonSerializer.Deserialize<JsonElement>(payload),
                            createdAt = notification?.CreatedAt ?? DateTime.UtcNow,
                            isRead = false
                        });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[SignalR] push failed for userId {UserId}", memberId);
                }
            }
        }

        public async Task HandleFamilyExpenseAddedAsync(FamilyExpenseEventMessage message)
            => await HandleFamilyExpenseNotificationAsync(message, NotificationType.FamilyExpenseAdded);

        public async Task HandleFamilyExpenseDeletedAsync(FamilyExpenseEventMessage message)
            => await HandleFamilyExpenseNotificationAsync(message, NotificationType.FamilyExpenseDeleted);

        private async Task HandleFamilyExpenseNotificationAsync(FamilyExpenseEventMessage message, string notifType)
        {
            var payload = JsonSerializer.Serialize(new
            {
                type = notifType,
                familyId = message.FamilyId,
                familyName = message.FamilyName,
                expenseId = message.ExpenseId,
                amount = message.Amount,
                currencyCode = message.CurrencyCode,
                actorName = message.ActorName,
                actorUserId = message.ActorUserId
            });

            foreach (var memberId in message.MemberUserIds)
            {
                Notification? notification = null;
                try
                {
                    notification = await _notifRepo.CreateAsync(new Notification
                    {
                        UserId = memberId,
                        Type = notifType,
                        Payload = payload,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[DB] notification persist failed for {Type}, userId {UserId}", notifType, memberId);
                }

                try
                {
                    await _hubContext.Clients
                        .Group(memberId.ToString())
                        .SendAsync("notification", new
                        {
                            id = notification?.Id,
                            type = notifType,
                            payload = JsonSerializer.Deserialize<JsonElement>(payload),
                            createdAt = notification?.CreatedAt ?? DateTime.UtcNow,
                            isRead = false
                        });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[SignalR] push failed for userId {UserId}", memberId);
                }
            }
        }

        public async Task HandleImportCompletedAsync(ImportCompletedEventMessage message)
        {
            var payload = JsonSerializer.Serialize(new
            {
                type = NotificationType.CsvImportCompleted,
                totalRows = message.TotalRows,
                importedCount = message.ImportedCount,
                skippedCount = message.SkippedCount
            });

            Notification? notification = null;
            try
            {
                notification = await _notifRepo.CreateAsync(new Notification
                {
                    UserId = message.UserId,
                    Type = NotificationType.CsvImportCompleted,
                    Payload = payload,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[DB] notification persist failed for import completed, userId {UserId}", message.UserId);
            }

            try
            {
                await _hubContext.Clients
                    .Group(message.UserId.ToString())
                    .SendAsync("notification", new
                    {
                        id = notification?.Id,
                        type = NotificationType.CsvImportCompleted,
                        payload = JsonSerializer.Deserialize<JsonElement>(payload),
                        createdAt = notification?.CreatedAt ?? DateTime.UtcNow,
                        isRead = false
                    });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[SignalR] push failed for userId {UserId}", message.UserId);
            }
        }

        public async Task HandleRateConflictAsync(RateConflictEventMessage message)
        {
            var payload = JsonSerializer.Serialize(new
            {
                type = NotificationType.RateConflictCreated,
                conflictId = message.ConflictId,
                sourceCurrencyCode = message.SourceCurrencyCode,
                destCurrencyCode = message.DestCurrencyCode,
                date = message.Date,
                autoRate = message.AutoRate,
                manualRate = message.ManualRate
            });

            foreach (var adminId in message.AdminUserIds)
            {
                Notification? notification = null;
                try
                {
                    notification = await _notifRepo.CreateAsync(new Notification
                    {
                        UserId = adminId,
                        Type = NotificationType.RateConflictCreated,
                        Payload = payload,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[DB] notification persist failed for rate conflict, userId {UserId}", adminId);
                }

                try
                {
                    await _hubContext.Clients
                        .Group(adminId.ToString())
                        .SendAsync("notification", new
                        {
                            id = notification?.Id,
                            type = NotificationType.RateConflictCreated,
                            payload = JsonSerializer.Deserialize<JsonElement>(payload),
                            createdAt = notification?.CreatedAt ?? DateTime.UtcNow,
                            isRead = false
                        });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[SignalR] push failed for userId {UserId}", adminId);
                }
            }
        }

        public Task HandleEmailVerificationAsync(UserNotificationEventMessage message)
        {
            try
            {
                var emailHtml = _emailHelper.GetEmailTemplate(
                    EmailHtmlTemplate.EmailVerification.Key,
                    new Dictionary<string, string>
                    {
                        { EmailHtmlTemplate.EmailVerification.Variables.VerificationLink, message.VerificationLink ?? string.Empty }
                    });
                _emailHelper.SendEmail(
                    recipientTo: message.Email,
                    emailSubject: "[Expenses Manager] Confirm your email address",
                    isHTML: true,
                    emailBody: emailHtml);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Email] verification email failed for {Email}", message.Email);
            }
            return Task.CompletedTask;
        }

        public Task HandlePasswordResetAsync(UserNotificationEventMessage message)
        {
            try
            {
                var emailHtml = _emailHelper.GetEmailTemplate(
                    EmailHtmlTemplate.PasswordReset.Key,
                    new Dictionary<string, string>
                    {
                        { EmailHtmlTemplate.PasswordReset.Variables.ResetLink, message.ResetLink ?? string.Empty }
                    });
                _emailHelper.SendEmail(
                    recipientTo: message.Email,
                    emailSubject: "[Expenses Manager] Password reset request",
                    isHTML: true,
                    emailBody: emailHtml);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Email] password reset email failed for {Email}", message.Email);
            }
            return Task.CompletedTask;
        }

        public Task HandlePasswordChangedAsync(UserNotificationEventMessage message)
        {
            try
            {
                var emailHtml = _emailHelper.GetEmailTemplate(
                    EmailHtmlTemplate.PasswordChanged.Key,
                    new Dictionary<string, string>
                    {
                        { EmailHtmlTemplate.PasswordChanged.Variables.FirstName, message.FirstName ?? string.Empty }
                    });
                _emailHelper.SendEmail(
                    recipientTo: message.Email,
                    emailSubject: "[Expenses Manager] Your password has been changed",
                    isHTML: true,
                    emailBody: emailHtml);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Email] password changed email failed for {Email}", message.Email);
            }
            return Task.CompletedTask;
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
