namespace Touir.ExpensesManager.Expenses.Messaging.Messages
{
    public class FamilyEventMessage
    {
        public string MessageId { get; set; } = null!;
        public string EventType { get; set; } = null!;
        public int TargetUserId { get; set; }
        public string TargetEmail { get; set; } = null!;
        public string TargetFirstName { get; set; } = null!;
        public int FamilyId { get; set; }
        public string FamilyName { get; set; } = null!;
        public int RemovedByUserId { get; set; }
        public string RemovedByName { get; set; } = null!;
        public int ExpenseCount { get; set; }
        public DateTime OccurredAt { get; set; }
    }

    public static class FamilyEventType
    {
        public const string MemberRemoved = "family.member.removed";
        public const string InvitationRequested = "family.invitation.requested";
        public const string InvitationAccepted = "family.invitation.accepted";
        public const string MemberJoined = "family.member.joined";
        public const string ExpenseAdded = "family.expense.added";
        public const string ExpenseDeleted = "family.expense.deleted";
        public const string ImportCompleted = "expenses.import.completed";
        public const string RateConflict = "expenses.rate.conflict";
    }
}
