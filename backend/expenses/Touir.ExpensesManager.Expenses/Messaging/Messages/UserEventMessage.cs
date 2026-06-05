namespace Touir.ExpensesManager.Expenses.Messaging.Messages
{
    public class UserEventMessage
    {
        public string EventType { get; set; } = null!;
        public int UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public int? FamilyId { get; set; }
        public bool IsAdmin { get; set; }
    }

    public static class UserEventType
    {
        public const string Created = "user.created";
        public const string Updated = "user.updated";
        public const string Deleted = "user.deleted";
    }
}
