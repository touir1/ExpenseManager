using RabbitMQ.Client;

namespace com.touir.expenses.Expenses.Services
{
    public interface IRabbitMQService
    {
        IConnection GetConnection();
    }
}
