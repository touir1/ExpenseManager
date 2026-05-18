using System.Diagnostics.CodeAnalysis;

namespace Touir.ExpensesManager.Users.Infrastructure.Options
{
    [ExcludeFromCodeCoverage]
    public class RabbitMQOptions
    {
        public string? HostName { get; set; }
        public int Port { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? VirtualHost { get; set; }
    }
}
