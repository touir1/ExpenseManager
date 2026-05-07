using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Touir.ExpensesManager.Expenses.Messaging.Messages;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Models.External;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;
using Touir.ExpensesManager.Expenses.Repositories.External.Contracts;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Messaging.Consumers
{
    public class UserEventConsumer : BackgroundService
    {
        private const string ExchangeName = "users.events";
        private const string QueueName = "expenses.users.sync";
        private const string RoutingKey = "user.#";

        private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IRabbitMQService _rabbitMqService;
        private readonly ILogger<UserEventConsumer> _logger;
        private IModel? _channel;

        public UserEventConsumer(
            IServiceScopeFactory scopeFactory,
            IRabbitMQService rabbitMqService,
            ILogger<UserEventConsumer> logger)
        {
            _scopeFactory = scopeFactory;
            _rabbitMqService = rabbitMqService;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
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

            return Task.CompletedTask;
        }

        private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs ea)
        {
            var body = Encoding.UTF8.GetString(ea.Body.ToArray());
            var messageId = ea.BasicProperties?.MessageId ?? Guid.NewGuid().ToString();
            try
            {
                var message = JsonSerializer.Deserialize<UserEventMessage>(body, _jsonOptions);

                if (message == null)
                {
                    _logger.LogWarning("Received null user event message, skipping.");
                    _channel!.BasicAck(ea.DeliveryTag, multiple: false);
                    return;
                }

                using var scope = _scopeFactory.CreateScope();
                var inboxRepo = scope.ServiceProvider.GetRequiredService<IInboxRepository>();

                if (await inboxRepo.ExistsAsync(messageId))
                {
                    _logger.LogInformation("Duplicate message {MessageId}, skipping.", messageId);
                    _channel!.BasicAck(ea.DeliveryTag, multiple: false);
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

                _channel!.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process user event message {MessageId}: {Body}", messageId, body);
                _channel!.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
            }
        }

        private async Task HandleMessageAsync(UserEventMessage message, IServiceScope scope)
        {
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            switch (message.EventType)
            {
                case UserEventType.Created:
                case UserEventType.Updated:
                    await userRepository.SaveOrUpdateUserAsync(new User
                    {
                        Id = message.UserId,
                        FirstName = message.FirstName,
                        LastName = message.LastName,
                        Email = message.Email,
                        FamilyId = message.FamilyId
                    });
                    break;

                case UserEventType.Deleted:
                    var user = await userRepository.GetUserByIdAsync(message.UserId);
                    if (user != null)
                        await userRepository.DeleteUserAsync(user);
                    break;

                default:
                    _logger.LogWarning("Unknown user event type: {EventType}", message.EventType);
                    break;
            }
        }

        public override void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();
            base.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
