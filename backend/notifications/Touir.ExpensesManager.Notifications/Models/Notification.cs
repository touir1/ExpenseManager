namespace Touir.ExpensesManager.Notifications.Models
{
    public class Notification
    {
        public long Id { get; set; }
        public int UserId { get; set; }
        public string Type { get; set; } = null!;
        public string Payload { get; set; } = null!;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
    }

    public static class NotificationType
    {
        public const string FamilyMemberRemoved = "FAMILY_MEMBER_REMOVED";
    }
}
