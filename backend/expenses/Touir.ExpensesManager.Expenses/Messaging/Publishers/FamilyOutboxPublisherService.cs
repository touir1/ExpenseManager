using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;

namespace Touir.ExpensesManager.Expenses.Messaging.Publishers
{
    [ExcludeFromCodeCoverage]
    public class FamilyOutboxPublisherService : BackgroundService
    {
        private const int PollIntervalMs = 5000;
        private const int MaxRetries = 5;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<FamilyOutboxPublisherService> _logger;

        public FamilyOutboxPublisherService(IServiceScopeFactory scopeFactory, ILogger<FamilyOutboxPublisherService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Family outbox publisher poll cycle failed.");
                }

                await Task.Delay(PollIntervalMs, stoppingToken);
            }
        }

        private async Task ProcessPendingAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var outboxRepo = scope.ServiceProvider.GetRequiredService<IExpensesOutboxRepository>();
            var publisher = scope.ServiceProvider.GetRequiredService<IFamilyEventPublisher>();

            var events = await outboxRepo.GetPendingAsync(MaxRetries);
            foreach (var evt in events)
            {
                try
                {
                    publisher.PublishRaw(evt.EventType, evt.Payload, evt.MessageId);
                    await outboxRepo.MarkPublishedAsync(evt.Id);
                    _logger.LogInformation("Published family outbox event {MessageId} ({EventType}).", evt.MessageId, evt.EventType);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to publish family outbox event {MessageId}, retry {RetryCount}/{MaxRetries}.",
                        evt.MessageId, evt.RetryCount + 1, MaxRetries);
                    await outboxRepo.MarkFailedAsync(evt.Id, ex.Message);
                }
            }
        }
    }
}
