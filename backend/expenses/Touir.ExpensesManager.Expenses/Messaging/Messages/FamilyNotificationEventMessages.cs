namespace Touir.ExpensesManager.Expenses.Messaging.Messages
{
    public class FamilyInvitationEventMessage
    {
        public string MessageId { get; set; } = null!;
        public string EventType { get; set; } = null!;
        public string InviteeEmail { get; set; } = null!;
        public string InviterName { get; set; } = null!;
        public string FamilyName { get; set; } = null!;
        public string InviteLink { get; set; } = null!;
    }

    public class FamilyInvitationAcceptedEventMessage
    {
        public string MessageId { get; set; } = null!;
        public string EventType { get; set; } = null!;
        public int HeadUserId { get; set; }
        public int FamilyId { get; set; }
        public string FamilyName { get; set; } = null!;
        public string AcceptorName { get; set; } = null!;
        public string AcceptorEmail { get; set; } = null!;
    }

    public class FamilyMemberJoinedEventMessage
    {
        public string MessageId { get; set; } = null!;
        public string EventType { get; set; } = null!;
        public int FamilyId { get; set; }
        public string FamilyName { get; set; } = null!;
        public string JoinerName { get; set; } = null!;
        public int JoinerUserId { get; set; }
        public List<int> MemberUserIds { get; set; } = [];
    }

    public class FamilyExpenseEventMessage
    {
        public string MessageId { get; set; } = null!;
        public string EventType { get; set; } = null!;
        public int FamilyId { get; set; }
        public string FamilyName { get; set; } = null!;
        public long ExpenseId { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = null!;
        public string ActorName { get; set; } = null!;
        public int ActorUserId { get; set; }
        public List<int> MemberUserIds { get; set; } = [];
    }

    public class ImportCompletedEventMessage
    {
        public string MessageId { get; set; } = null!;
        public string EventType { get; set; } = null!;
        public int UserId { get; set; }
        public int TotalRows { get; set; }
        public int ImportedCount { get; set; }
        public int SkippedCount { get; set; }
    }

    public class RateConflictEventMessage
    {
        public string MessageId { get; set; } = null!;
        public string EventType { get; set; } = null!;
        public long ConflictId { get; set; }
        public string SourceCurrencyCode { get; set; } = null!;
        public string DestCurrencyCode { get; set; } = null!;
        public DateOnly Date { get; set; }
        public decimal AutoRate { get; set; }
        public decimal ManualRate { get; set; }
        public List<int> AdminUserIds { get; set; } = [];
    }
}
