using RabbitMQ.Client;

namespace Touir.ExpensesManager.Expenses.Services.Contracts
{
    public interface IRabbitMQService
    {
        IConnection GetConnection();
    }
}
