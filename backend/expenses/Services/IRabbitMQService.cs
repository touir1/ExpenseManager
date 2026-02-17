using RabbitMQ.Client;

namespace Touir.ExpensesManager.Expenses.Services
{
    public interface IRabbitMQService
    {
        IConnection GetConnection();
    }
}
