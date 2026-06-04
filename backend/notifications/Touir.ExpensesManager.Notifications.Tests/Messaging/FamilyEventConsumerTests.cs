using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Touir.ExpensesManager.Notifications.Messaging.Consumers;
using Touir.ExpensesManager.Notifications.Messaging.Messages;
using Touir.ExpensesManager.Notifications.Models;
using Touir.ExpensesManager.Notifications.Repositories.Contracts;
using Touir.ExpensesManager.Notifications.Services.Contracts;

namespace Touir.ExpensesManager.Notifications.Tests.Messaging
{
    public class FamilyEventConsumerTests
    {
        private static FamilyEventMessage MakeMessage(string eventType = FamilyEventType.MemberRemoved) => new()
        {
            MessageId = "msg-123",
            EventType = eventType,
            TargetUserId = 1,
            TargetEmail = "user@test.com",
            TargetFirstName = "Alice",
            FamilyId = 10,
            FamilyName = "Family",
            RemovedByUserId = 2,
            RemovedByName = "Bob",
            ExpenseCount = 2,
            OccurredAt = DateTime.UtcNow
        };

        private static BasicDeliverEventArgs MakeDelivery(FamilyEventMessage message, string? messageId = null)
        {
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = new Mock<RabbitMQ.Client.IBasicProperties>();
            properties.Setup(p => p.MessageId).Returns(messageId ?? message.MessageId);

            return new BasicDeliverEventArgs
            {
                Body = body,
                BasicProperties = properties.Object,
                DeliveryTag = 1
            };
        }

        private static (
            TestableConsumer consumer,
            Mock<IInboxRepository> inboxMock,
            Mock<INotificationService> serviceMock)
            CreateSut()
        {
            var inboxMock = new Mock<IInboxRepository>();
            var serviceMock = new Mock<INotificationService>();

            inboxMock.Setup(r => r.ExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            inboxMock.Setup(r => r.AddAsync(It.IsAny<InboxEvent>())).Returns(Task.CompletedTask);
            serviceMock.Setup(s => s.HandleFamilyMemberRemovedAsync(It.IsAny<FamilyEventMessage>()))
                .Returns(Task.CompletedTask);

            var services = new ServiceCollection();
            services.AddSingleton(inboxMock.Object);
            services.AddSingleton(serviceMock.Object);
            var provider = services.BuildServiceProvider();

            var scopeFactory = new Mock<IServiceScopeFactory>();
            var scope = new Mock<IServiceScope>();
            scope.Setup(s => s.ServiceProvider).Returns(provider);
            scopeFactory.Setup(f => f.CreateScope()).Returns(scope.Object);

            var consumer = new TestableConsumer(
                scopeFactory.Object,
                NullLogger<FamilyEventConsumer>.Instance);

            return (consumer, inboxMock, serviceMock);
        }

        [Fact]
        public async Task OnMessageReceived_ValidMessage_CallsHandlerAndAcks()
        {
            var (consumer, inboxMock, serviceMock) = CreateSut();
            var ea = MakeDelivery(MakeMessage());

            await consumer.InvokeOnMessageReceived(ea);

            serviceMock.Verify(s => s.HandleFamilyMemberRemovedAsync(
                It.Is<FamilyEventMessage>(m => m.MessageId == "msg-123")), Times.Once);
            inboxMock.Verify(r => r.AddAsync(It.IsAny<InboxEvent>()), Times.Once);
            Assert.True(consumer.WasAcked);
            Assert.False(consumer.WasNacked);
        }

        [Fact]
        public async Task OnMessageReceived_DuplicateMessageId_AcksAndSkips()
        {
            var (consumer, inboxMock, serviceMock) = CreateSut();
            inboxMock.Setup(r => r.ExistsAsync("msg-123")).ReturnsAsync(true);
            var ea = MakeDelivery(MakeMessage());

            await consumer.InvokeOnMessageReceived(ea);

            serviceMock.Verify(s => s.HandleFamilyMemberRemovedAsync(It.IsAny<FamilyEventMessage>()), Times.Never);
            inboxMock.Verify(r => r.AddAsync(It.IsAny<InboxEvent>()), Times.Never);
            Assert.True(consumer.WasAcked);
        }

        [Fact]
        public async Task OnMessageReceived_ServiceThrows_Nacks()
        {
            var (consumer, _, serviceMock) = CreateSut();
            serviceMock.Setup(s => s.HandleFamilyMemberRemovedAsync(It.IsAny<FamilyEventMessage>()))
                .ThrowsAsync(new Exception("service failure"));
            var ea = MakeDelivery(MakeMessage());

            await consumer.InvokeOnMessageReceived(ea);

            Assert.True(consumer.WasNacked);
            Assert.False(consumer.WasAcked);
        }

        [Fact]
        public async Task OnMessageReceived_UnknownEventType_AcksAfterInboxWrite()
        {
            var (consumer, inboxMock, serviceMock) = CreateSut();
            var msg = MakeMessage("family.some.unknown");
            var ea = MakeDelivery(msg);

            await consumer.InvokeOnMessageReceived(ea);

            serviceMock.Verify(s => s.HandleFamilyMemberRemovedAsync(It.IsAny<FamilyEventMessage>()), Times.Never);
            inboxMock.Verify(r => r.AddAsync(It.IsAny<InboxEvent>()), Times.Once);
            Assert.True(consumer.WasAcked);
        }
    }

    /// <summary>Exposes OnMessageReceivedAsync and replaces the RabbitMQ channel with test stubs.</summary>
    public sealed class TestableConsumer : FamilyEventConsumer
    {
        public bool WasAcked { get; private set; }
        public bool WasNacked { get; private set; }

        public TestableConsumer(IServiceScopeFactory scopeFactory, Microsoft.Extensions.Logging.ILogger<FamilyEventConsumer> logger)
            : base(scopeFactory, new StubRabbitMQService(), logger) { }

        public Task InvokeOnMessageReceived(BasicDeliverEventArgs ea)
            => OnMessageReceivedAsync(this, ea);

        protected override void Ack(ulong deliveryTag) => WasAcked = true;
        protected override void Nack(ulong deliveryTag) => WasNacked = true;
    }

    public class StubRabbitMQService : Touir.ExpensesManager.Notifications.Services.Contracts.IRabbitMQService
    {
        public RabbitMQ.Client.IConnection GetConnection() => throw new NotSupportedException();
    }
}
