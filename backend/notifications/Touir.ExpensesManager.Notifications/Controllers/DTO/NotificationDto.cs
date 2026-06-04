using System.Text.Json;

namespace Touir.ExpensesManager.Notifications.Controllers.DTO
{
    public class NotificationDto
    {
        public long Id { get; set; }
        public string Type { get; set; } = null!;
        public JsonElement Payload { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}
