﻿using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Expenses.Services
{
    public class RabbitMQService : IRabbitMQService
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly RabbitMQOption _option;

        public RabbitMQService(IOptions<RabbitMQOption> option)
        {
            _option = option.Value;

            _connectionFactory = new ConnectionFactory
            {
                HostName = _option.HostName,
                Port = _option.Port,
                UserName = _option.UserName,
                Password = _option.Password
            };
        }

        public IConnection GetConnection()
        {
            return _connectionFactory.CreateConnection();
        }
    }
}