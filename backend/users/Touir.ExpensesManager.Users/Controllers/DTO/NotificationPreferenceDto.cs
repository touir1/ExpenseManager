namespace Touir.ExpensesManager.Users.Controllers.DTO
{
    public class NotificationPreferenceDto
    {
        public string EventType { get; set; } = null!;
        public bool EmailEnabled { get; set; }
    }
}
