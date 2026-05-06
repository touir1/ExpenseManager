namespace Touir.ExpensesManager.Expenses.Models
{
    public class InboxEvent
    {
        public string MessageId { get; set; } = null!;
        public string EventType { get; set; } = null!;
        public DateTime ReceivedAt { get; set; }
        public string Status { get; set; } = null!;
        public string? Error { get; set; }
    }

    public static class InboxEventStatus
    {
        public const string Processed = "Processed";
        public const string Failed = "Failed";
    }
}
