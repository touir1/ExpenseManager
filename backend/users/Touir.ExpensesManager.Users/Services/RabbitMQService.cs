using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Touir.ExpensesManager.Users.Infrastructure.Options;
using Touir.ExpensesManager.Users.Services.Contracts;

namespace Touir.ExpensesManager.Users.Services
{
    [ExcludeFromCodeCoverage]
    public class RabbitMQService : IRabbitMQService
    {
        private readonly ConnectionFactory _connectionFactory;
        private IConnection? _connection;
        private readonly object _lock = new();

        public RabbitMQService(IOptions<RabbitMQOptions> options)
        {
            var opt = options.Value;
            _connectionFactory = new ConnectionFactory
            {
                HostName = opt.HostName,
                Port = opt.Port,
                UserName = opt.UserName,
                Password = opt.Password,
                VirtualHost = opt.VirtualHost ?? "/"
            };
        }

        public IConnection GetConnection()
        {
            if (_connection is { IsOpen: true })
                return _connection;

            lock (_lock)
            {
                if (_connection is { IsOpen: true })
                    return _connection;

                _connection = _connectionFactory.CreateConnection();
            }

            return _connection;
        }
    }
}
