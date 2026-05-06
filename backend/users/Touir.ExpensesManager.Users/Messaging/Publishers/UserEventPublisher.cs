using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Touir.ExpensesManager.Users.Messaging.Messages;
using Touir.ExpensesManager.Users.Services.Contracts;

namespace Touir.ExpensesManager.Users.Messaging.Publishers
{
    public class UserEventPublisher : IUserEventPublisher
    {
        private const string ExchangeName = "users.events";
        private readonly IRabbitMQService _rabbitMqService;

        public UserEventPublisher(IRabbitMQService rabbitMqService)
        {
            _rabbitMqService = rabbitMqService;
        }

        public void Publish(UserEventMessage message)
        {
            var json = JsonSerializer.Serialize(message);
            PublishRaw(message.EventType, json, Guid.NewGuid().ToString());
        }

        public void PublishRaw(string eventType, string jsonPayload, string messageId)
        {
            using var channel = _rabbitMqService.GetConnection().CreateModel();
            channel.ExchangeDeclare(ExchangeName, ExchangeType.Topic, durable: true, autoDelete: false);

            var body = Encoding.UTF8.GetBytes(jsonPayload);
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.MessageId = messageId;

            channel.BasicPublish(ExchangeName, eventType, mandatory: false, basicProperties: properties, body: body);
        }
    }
}
