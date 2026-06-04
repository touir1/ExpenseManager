using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Touir.ExpensesManager.Expenses.Messaging.Messages;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Messaging.Publishers
{
    public class FamilyEventPublisher : IFamilyEventPublisher
    {
        private const string ExchangeName = "expenses.events";
        private readonly IRabbitMQService _rabbitMqService;

        public FamilyEventPublisher(IRabbitMQService rabbitMqService)
        {
            _rabbitMqService = rabbitMqService;
        }

        public void Publish(FamilyEventMessage message)
        {
            var json = JsonSerializer.Serialize(message);
            PublishRaw(message.EventType, json, message.MessageId);
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
