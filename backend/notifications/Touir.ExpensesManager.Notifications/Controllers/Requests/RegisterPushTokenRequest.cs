namespace Touir.ExpensesManager.Notifications.Controllers.Requests
{
    public class RegisterPushTokenRequest
    {
        public string Token { get; set; } = string.Empty;
        public string? DeviceId { get; set; }
    }
}
