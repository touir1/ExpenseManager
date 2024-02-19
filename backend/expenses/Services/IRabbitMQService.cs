using RabbitMQ.Client;

namespace Expenses.Services
{
    public interface IRabbitMQService
    {
        IConnection GetConnection();
    }
}
