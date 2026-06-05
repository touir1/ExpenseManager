namespace Touir.ExpensesManager.Notifications.Messaging.Messages
{
    public class UserNotificationEventMessage
    {
        public string MessageId { get; set; } = null!;
        public string EventType { get; set; } = null!;
        public int UserId { get; set; }
        public string Email { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? VerificationLink { get; set; }
        public string? ResetLink { get; set; }
        public string? AppCode { get; set; }
    }

    public static class UserEventType
    {
        public const string EmailVerificationRequested = "user.email.verification.requested";
        public const string PasswordResetRequested = "user.password.reset.requested";
        public const string PasswordChanged = "user.password.changed";
    }
}
