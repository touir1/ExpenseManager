namespace Touir.ExpensesManager.Notifications.Infrastructure.Options
{
    public class EmailOptions
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Host { get; set; }
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
    }
}
