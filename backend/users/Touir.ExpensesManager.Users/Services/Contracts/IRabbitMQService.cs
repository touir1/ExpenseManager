using RabbitMQ.Client;

namespace Touir.ExpensesManager.Users.Services.Contracts
{
    public interface IRabbitMQService
    {
        IConnection GetConnection();
    }
}
