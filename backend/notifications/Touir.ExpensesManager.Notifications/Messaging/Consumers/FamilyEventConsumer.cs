using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Text;
using System.Text.Json;
using Touir.ExpensesManager.Notifications.Messaging.Messages;
using Touir.ExpensesManager.Notifications.Models;
using Touir.ExpensesManager.Notifications.Repositories.Contracts;
using Touir.ExpensesManager.Notifications.Services.Contracts;

namespace Touir.ExpensesManager.Notifications.Messaging.Consumers
{
    [ExcludeFromCodeCoverage]
    public class FamilyEventConsumer : BackgroundService
    {
        private const string ExchangeName = "expenses.events";
        private const string QueueName = "notifications.expenses.sync";
        private const string RoutingKey = "family.#";

        private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        protected readonly IServiceScopeFactory _scopeFactory;
        protected readonly IRabbitMQService _rabbitMqService;
        protected readonly ILogger<FamilyEventConsumer> _logger;
        private IModel? _channel;

        public FamilyEventConsumer(
            IServiceScopeFactory scopeFactory,
            IRabbitMQService rabbitMqService,
            ILogger<FamilyEventConsumer> logger)
        {
            _scopeFactory = scopeFactory;
            _rabbitMqService = rabbitMqService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _channel = _rabbitMqService.GetConnection().CreateModel();
                    _channel.ExchangeDeclare(ExchangeName, ExchangeType.Topic, durable: true, autoDelete: false);
                    _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false);
                    _channel.QueueBind(QueueName, ExchangeName, RoutingKey);
                    _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                    var consumer = new AsyncEventingBasicConsumer(_channel);
                    consumer.Received += OnMessageReceivedAsync;
                    _channel.BasicConsume(QueueName, autoAck: false, consumer);

                    stoppingToken.Register(() => _channel?.Close());
                    _logger.LogInformation("FamilyEventConsumer connected to RabbitMQ.");
                    return;
                }
                catch (BrokerUnreachableException ex)
                {
                    _logger.LogWarning(ex, "RabbitMQ unreachable. Retrying in 5s...");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }

        protected async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs ea)
        {
            var body = Encoding.UTF8.GetString(ea.Body.ToArray());
            var messageId = ea.BasicProperties?.MessageId ?? Guid.NewGuid().ToString();

            try
            {
                var message = JsonSerializer.Deserialize<FamilyEventMessage>(body, _jsonOptions);

                if (message == null)
                {
                    _logger.LogWarning("Received null family event message, skipping.");
                    Ack(ea.DeliveryTag);
                    return;
                }

                using var scope = _scopeFactory.CreateScope();
                var inboxRepo = scope.ServiceProvider.GetRequiredService<IInboxRepository>();

                if (await inboxRepo.ExistsAsync(messageId))
                {
                    _logger.LogInformation("Duplicate message {MessageId}, skipping.", messageId);
                    Ack(ea.DeliveryTag);
                    return;
                }

                await HandleMessageAsync(message, scope);

                await inboxRepo.AddAsync(new InboxEvent
                {
                    MessageId = messageId,
                    EventType = message.EventType,
                    ReceivedAt = DateTime.UtcNow,
                    Status = InboxEventStatus.Processed
                });

                Ack(ea.DeliveryTag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process family event message {MessageId}: {Body}", messageId, body);
                Nack(ea.DeliveryTag);
            }
        }

        private async Task HandleMessageAsync(FamilyEventMessage message, IServiceScope scope)
        {
            switch (message.EventType)
            {
                case FamilyEventType.MemberRemoved:
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                    await notificationService.HandleFamilyMemberRemovedAsync(message);
                    break;

                default:
                    _logger.LogWarning("Unknown family event type: {EventType}", message.EventType);
                    break;
            }
        }

        protected virtual void Ack(ulong deliveryTag) =>
            _channel?.BasicAck(deliveryTag, multiple: false);

        protected virtual void Nack(ulong deliveryTag) =>
            _channel?.BasicNack(deliveryTag, multiple: false, requeue: false);

        public override void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();
            base.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
