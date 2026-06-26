using Touir.ExpensesManager.Users.Controllers.DTO;

namespace Touir.ExpensesManager.Users.Controllers.Requests
{
    public class UpdateNotificationPreferencesRequest
    {
        public required IEnumerable<NotificationPreferenceDto> Preferences { get; set; }
    }
}
