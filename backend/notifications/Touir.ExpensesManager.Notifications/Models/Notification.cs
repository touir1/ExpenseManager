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
        public const string FamilyInvitationAccepted = "FAMILY_INVITATION_ACCEPTED";
        public const string FamilyMemberJoined = "FAMILY_MEMBER_JOINED";
        public const string FamilyExpenseAdded = "FAMILY_EXPENSE_ADDED";
        public const string FamilyExpenseDeleted = "FAMILY_EXPENSE_DELETED";
        public const string CsvImportCompleted = "CSV_IMPORT_COMPLETED";
        public const string RateConflictCreated = "RATE_CONFLICT_CREATED";
    }
}
