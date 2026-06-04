namespace Touir.ExpensesManager.Expenses.Models
{
    public class OutboxEvent
    {
        public long Id { get; set; }
        public string MessageId { get; set; } = null!;
        public string EventType { get; set; } = null!;
        public string Payload { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public int RetryCount { get; set; }
        public string? LastError { get; set; }
    }
}
