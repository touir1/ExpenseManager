namespace Touir.ExpensesManager.Users.Messaging.Messages
{
    public class UserNotificationEventMessage
    {
        public string EventType { get; set; } = null!;
        public int UserId { get; set; }
        public string Email { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? VerificationLink { get; set; }
        public string? ResetLink { get; set; }
        public string? AppCode { get; set; }
    }
}
