namespace Touir.ExpensesManager.Users.Models
{
    public class NotificationPreference
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string EventType { get; set; } = null!;
        public bool EmailEnabled { get; set; }
    }
}
