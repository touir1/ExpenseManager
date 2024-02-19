using RabbitMQ.Client;

namespace Expenses.Services
{
    public class RabbitMQService : IRabbitMQService
    {
        private readonly ConnectionFactory _connectionFactory;

        public RabbitMQService(string hostName, int port, string userName, string password)
        {
            _connectionFactory = new ConnectionFactory
            {
                HostName = hostName,
                Port = port,
                UserName = userName,
                Password = password
            };
        }

        public IConnection GetConnection()
        {
            return _connectionFactory.CreateConnection();
        }
    }
}
