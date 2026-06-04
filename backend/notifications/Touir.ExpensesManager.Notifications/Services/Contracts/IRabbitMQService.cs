using RabbitMQ.Client;

namespace Touir.ExpensesManager.Notifications.Services.Contracts
{
    public interface IRabbitMQService
    {
        IConnection GetConnection();
    }
}
