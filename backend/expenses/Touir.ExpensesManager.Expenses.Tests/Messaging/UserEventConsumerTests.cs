using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Touir.ExpensesManager.Expenses.Messaging.Consumers;
using Touir.ExpensesManager.Expenses.Messaging.Messages;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Models.External;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;
using Touir.ExpensesManager.Expenses.Repositories.External.Contracts;
using Touir.ExpensesManager.Expenses.Services;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Tests.Messaging
{
    public class UserEventConsumerTests
    {
        private readonly Mock<IServiceScopeFactory> _scopeFactoryMock = new();
        private readonly Mock<IRabbitMQService> _rabbitMqServiceMock = new();
        private readonly Mock<ILogger<UserEventConsumer>> _loggerMock = new();
        private readonly Mock<IModel> _channelMock = new();
        private readonly Mock<IConnection> _connectionMock = new();

        public UserEventConsumerTests()
        {
            _connectionMock.Setup(c => c.CreateModel()).Returns(_channelMock.Object);
            _rabbitMqServiceMock.Setup(r => r.GetConnection()).Returns(_connectionMock.Object);
            // BasicConsume extension methods delegate to this full IModel signature
            _channelMock.Setup(c => c.BasicConsume(
                It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(),
                It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>(),
                It.IsAny<IBasicConsumer>()))
                .Returns("consumer-tag");
        }

        private UserEventConsumer CreateConsumer()
            => new(_scopeFactoryMock.Object, _rabbitMqServiceMock.Object, _loggerMock.Object);

        private static void SetChannel(UserEventConsumer consumer, IModel? channel)
        {
            typeof(UserEventConsumer)
                .GetField("_channel", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(consumer, channel);
        }

        private static Task InvokeOnMessageReceived(UserEventConsumer consumer, BasicDeliverEventArgs ea)
        {
            var method = typeof(UserEventConsumer)
                .GetMethod("OnMessageReceivedAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;
            return (Task)method.Invoke(consumer, [new object(), ea])!;
        }

        private static Task InvokeHandleMessage(UserEventConsumer consumer, UserEventMessage message, IServiceScope scope)
        {
            var method = typeof(UserEventConsumer)
                .GetMethod("HandleMessageAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;
            return (Task)method.Invoke(consumer, [message, scope])!;
        }

        private (Mock<IServiceScope>, Mock<IInboxRepository>, Mock<IUserRepository>, Mock<IFamilyService>) SetupScope()
        {
            var scopeMock = new Mock<IServiceScope>();
            var inboxRepoMock = new Mock<IInboxRepository>();
            var userRepoMock = new Mock<IUserRepository>();
            var familyServiceMock = new Mock<IFamilyService>();
            familyServiceMock.Setup(f => f.CreateDefaultAsync(It.IsAny<int>())).Returns(Task.CompletedTask);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IInboxRepository))).Returns(inboxRepoMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IUserRepository))).Returns(userRepoMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IFamilyService))).Returns(familyServiceMock.Object);
            scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);
            _scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

            return (scopeMock, inboxRepoMock, userRepoMock, familyServiceMock);
        }

        private static BasicDeliverEventArgs CreateEventArgs(object body, string? messageId = "test-msg-id", ulong deliveryTag = 1)
        {
            var bytes = body is string s
                ? Encoding.UTF8.GetBytes(s)
                : JsonSerializer.SerializeToUtf8Bytes(body);
            var propsMock = new Mock<IBasicProperties>();
            propsMock.Setup(p => p.MessageId).Returns(messageId!);
            return new BasicDeliverEventArgs
            {
                Body = bytes,
                BasicProperties = propsMock.Object,
                DeliveryTag = deliveryTag
            };
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_InitializesSuccessfully()
        {
            var consumer = CreateConsumer();
            Assert.NotNull(consumer);
        }

        #endregion

        #region ExecuteAsync Tests

        [Fact]
        public async Task ExecuteAsync_SuccessfulConnection_SetsUpChannelAndReturns()
        {
            using var consumer = CreateConsumer();
            using var cts = new CancellationTokenSource();

            await consumer.StartAsync(cts.Token);

            _connectionMock.Verify(c => c.CreateModel(), Times.Once);
            _channelMock.Verify(c => c.ExchangeDeclare(
                "users.events", ExchangeType.Topic, true, false,
                It.IsAny<IDictionary<string, object>>()), Times.Once);
            _channelMock.Verify(c => c.QueueDeclare(
                "expenses.users.sync", true, false, false,
                It.IsAny<IDictionary<string, object>>()), Times.Once);
            _channelMock.Verify(c => c.QueueBind(
                "expenses.users.sync", "users.events", "user.#",
                It.IsAny<IDictionary<string, object>>()), Times.Once);
            _channelMock.Verify(c => c.BasicQos(0, 1, false), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_CancelledBeforeStart_DoesNotConnect()
        {
            using var consumer = CreateConsumer();
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await consumer.StartAsync(cts.Token);

            _rabbitMqServiceMock.Verify(r => r.GetConnection(), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_BrokerUnreachable_LogsWarningAndRetries()
        {
            _rabbitMqServiceMock
                .SetupSequence(r => r.GetConnection())
                .Throws(new BrokerUnreachableException(new Exception("broker down")))
                .Returns(_connectionMock.Object);

            using var consumer = CreateConsumer();
            using var cts = new CancellationTokenSource(50);

            try { await consumer.StartAsync(cts.Token); } catch { }

            _rabbitMqServiceMock.Verify(r => r.GetConnection(), Times.AtLeastOnce);
        }

        #endregion

        #region Dispose Tests

        [Fact]
        public void Dispose_WithChannel_ClosesAndDisposesChannel()
        {
            var consumer = CreateConsumer();
            SetChannel(consumer, _channelMock.Object);

            consumer.Dispose();

            _channelMock.Verify(c => c.Close(), Times.Once);
            _channelMock.Verify(c => c.Dispose(), Times.Once);
        }

        [Fact]
        public void Dispose_WithNullChannel_DoesNotThrow()
        {
            var consumer = CreateConsumer();

            var ex = Record.Exception(() => consumer.Dispose());

            Assert.Null(ex);
        }

        #endregion

        #region OnMessageReceivedAsync Tests

        [Fact]
        public async Task OnMessageReceived_NullDeserializedMessage_AcksAndSkips()
        {
            var consumer = CreateConsumer();
            SetChannel(consumer, _channelMock.Object);

            var ea = CreateEventArgs("null");

            await InvokeOnMessageReceived(consumer, ea);

            _channelMock.Verify(c => c.BasicAck(1, false), Times.Once);
            _scopeFactoryMock.Verify(f => f.CreateScope(), Times.Never);
        }

        [Fact]
        public async Task OnMessageReceived_DuplicateMessage_AcksWithoutProcessing()
        {
            var (_, inboxRepoMock, userRepoMock, _) = SetupScope();
            inboxRepoMock.Setup(r => r.ExistsAsync("dup-id")).ReturnsAsync(true);

            var consumer = CreateConsumer();
            SetChannel(consumer, _channelMock.Object);

            var msg = new UserEventMessage { EventType = UserEventType.Created, UserId = 1 };
            var ea = CreateEventArgs(msg, "dup-id");

            await InvokeOnMessageReceived(consumer, ea);

            userRepoMock.Verify(r => r.SaveOrUpdateUserAsync(It.IsAny<User>()), Times.Never);
            _channelMock.Verify(c => c.BasicAck(1, false), Times.Once);
        }

        [Fact]
        public async Task OnMessageReceived_CreatedEvent_SavesUserAndAcks()
        {
            var (_, inboxRepoMock, userRepoMock, _) = SetupScope();
            inboxRepoMock.Setup(r => r.ExistsAsync("msg-c1")).ReturnsAsync(false);
            userRepoMock.Setup(r => r.SaveOrUpdateUserAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            inboxRepoMock.Setup(r => r.AddAsync(It.IsAny<InboxEvent>())).Returns(Task.CompletedTask);

            var consumer = CreateConsumer();
            SetChannel(consumer, _channelMock.Object);

            var msg = new UserEventMessage
            {
                EventType = UserEventType.Created,
                UserId = 42,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                FamilyId = 5
            };
            var ea = CreateEventArgs(msg, "msg-c1");

            await InvokeOnMessageReceived(consumer, ea);

            userRepoMock.Verify(r => r.SaveOrUpdateUserAsync(It.Is<User>(u =>
                u.Id == 42 && u.FirstName == "John" && u.Email == "john@example.com" && u.FamilyId == 5)),
                Times.Once);
            inboxRepoMock.Verify(r => r.AddAsync(It.Is<InboxEvent>(e =>
                e.MessageId == "msg-c1" && e.EventType == UserEventType.Created && e.Status == InboxEventStatus.Processed)),
                Times.Once);
            _channelMock.Verify(c => c.BasicAck(1, false), Times.Once);
        }

        [Fact]
        public async Task OnMessageReceived_UpdatedEvent_SavesUserAndAcks()
        {
            var (_, inboxRepoMock, userRepoMock, _) = SetupScope();
            inboxRepoMock.Setup(r => r.ExistsAsync("msg-u1")).ReturnsAsync(false);
            userRepoMock.Setup(r => r.SaveOrUpdateUserAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            inboxRepoMock.Setup(r => r.AddAsync(It.IsAny<InboxEvent>())).Returns(Task.CompletedTask);

            var consumer = CreateConsumer();
            SetChannel(consumer, _channelMock.Object);

            var msg = new UserEventMessage
            {
                EventType = UserEventType.Updated,
                UserId = 10,
                Email = "updated@example.com"
            };
            var ea = CreateEventArgs(msg, "msg-u1");

            await InvokeOnMessageReceived(consumer, ea);

            userRepoMock.Verify(r => r.SaveOrUpdateUserAsync(It.Is<User>(u => u.Id == 10)), Times.Once);
            _channelMock.Verify(c => c.BasicAck(1, false), Times.Once);
        }

        [Fact]
        public async Task OnMessageReceived_DeletedEvent_ExistingUser_DeletesAndAcks()
        {
            var (_, inboxRepoMock, userRepoMock, _) = SetupScope();
            inboxRepoMock.Setup(r => r.ExistsAsync("msg-d1")).ReturnsAsync(false);
            var existingUser = new User { Id = 7 };
            userRepoMock.Setup(r => r.GetUserByIdAsync(7)).ReturnsAsync(existingUser);
            userRepoMock.Setup(r => r.DeleteUserAsync(existingUser)).Returns(Task.CompletedTask);
            inboxRepoMock.Setup(r => r.AddAsync(It.IsAny<InboxEvent>())).Returns(Task.CompletedTask);

            var consumer = CreateConsumer();
            SetChannel(consumer, _channelMock.Object);

            var msg = new UserEventMessage { EventType = UserEventType.Deleted, UserId = 7 };
            var ea = CreateEventArgs(msg, "msg-d1");

            await InvokeOnMessageReceived(consumer, ea);

            userRepoMock.Verify(r => r.DeleteUserAsync(existingUser), Times.Once);
            _channelMock.Verify(c => c.BasicAck(1, false), Times.Once);
        }

        [Fact]
        public async Task OnMessageReceived_DeletedEvent_NonExistingUser_SkipsDeleteAndAcks()
        {
            var (_, inboxRepoMock, userRepoMock, _) = SetupScope();
            inboxRepoMock.Setup(r => r.ExistsAsync("msg-d2")).ReturnsAsync(false);
            userRepoMock.Setup(r => r.GetUserByIdAsync(99)).ReturnsAsync((User?)null);
            inboxRepoMock.Setup(r => r.AddAsync(It.IsAny<InboxEvent>())).Returns(Task.CompletedTask);

            var consumer = CreateConsumer();
            SetChannel(consumer, _channelMock.Object);

            var msg = new UserEventMessage { EventType = UserEventType.Deleted, UserId = 99 };
            var ea = CreateEventArgs(msg, "msg-d2");

            await InvokeOnMessageReceived(consumer, ea);

            userRepoMock.Verify(r => r.DeleteUserAsync(It.IsAny<User>()), Times.Never);
            _channelMock.Verify(c => c.BasicAck(1, false), Times.Once);
        }

        [Fact]
        public async Task OnMessageReceived_UnknownEventType_LogsWarningAndAcks()
        {
            var (_, inboxRepoMock, _, _) = SetupScope();
            inboxRepoMock.Setup(r => r.ExistsAsync("msg-x1")).ReturnsAsync(false);
            inboxRepoMock.Setup(r => r.AddAsync(It.IsAny<InboxEvent>())).Returns(Task.CompletedTask);

            var consumer = CreateConsumer();
            SetChannel(consumer, _channelMock.Object);

            var msg = new UserEventMessage { EventType = "user.unknown", UserId = 1 };
            var ea = CreateEventArgs(msg, "msg-x1");

            await InvokeOnMessageReceived(consumer, ea);

            _channelMock.Verify(c => c.BasicAck(1, false), Times.Once);
        }

        [Fact]
        public async Task OnMessageReceived_Exception_NacksMessage()
        {
            var (_, inboxRepoMock, _, _) = SetupScope();
            inboxRepoMock.Setup(r => r.ExistsAsync(It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            var consumer = CreateConsumer();
            SetChannel(consumer, _channelMock.Object);

            var msg = new UserEventMessage { EventType = UserEventType.Created, UserId = 1 };
            var ea = CreateEventArgs(msg);

            await InvokeOnMessageReceived(consumer, ea);

            _channelMock.Verify(c => c.BasicNack(1, false, false), Times.Once);
            _channelMock.Verify(c => c.BasicAck(It.IsAny<ulong>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task OnMessageReceived_NullMessageId_UsesGeneratedGuid()
        {
            var (_, inboxRepoMock, userRepoMock, _) = SetupScope();
            inboxRepoMock.Setup(r => r.ExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            userRepoMock.Setup(r => r.SaveOrUpdateUserAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            inboxRepoMock.Setup(r => r.AddAsync(It.IsAny<InboxEvent>())).Returns(Task.CompletedTask);

            var consumer = CreateConsumer();
            SetChannel(consumer, _channelMock.Object);

            var propsMock = new Mock<IBasicProperties>();
            propsMock.Setup(p => p.MessageId).Returns((string)null!);
            var bytes = JsonSerializer.SerializeToUtf8Bytes(new UserEventMessage { EventType = UserEventType.Created, UserId = 1 });
            var ea = new BasicDeliverEventArgs
            {
                Body = bytes,
                BasicProperties = propsMock.Object,
                DeliveryTag = 1
            };

            await InvokeOnMessageReceived(consumer, ea);

            inboxRepoMock.Verify(r => r.ExistsAsync(It.Is<string>(id => id != null && id.Length == 36)), Times.Once);
        }

        [Fact]
        public async Task OnMessageReceived_NullBasicProperties_UsesGeneratedGuid()
        {
            var (_, inboxRepoMock, userRepoMock, _) = SetupScope();
            inboxRepoMock.Setup(r => r.ExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            userRepoMock.Setup(r => r.SaveOrUpdateUserAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            inboxRepoMock.Setup(r => r.AddAsync(It.IsAny<InboxEvent>())).Returns(Task.CompletedTask);

            var consumer = CreateConsumer();
            SetChannel(consumer, _channelMock.Object);

            var bytes = JsonSerializer.SerializeToUtf8Bytes(new UserEventMessage { EventType = UserEventType.Created, UserId = 1 });
            var ea = new BasicDeliverEventArgs
            {
                Body = bytes,
                BasicProperties = null,
                DeliveryTag = 1
            };

            await InvokeOnMessageReceived(consumer, ea);

            inboxRepoMock.Verify(r => r.ExistsAsync(It.Is<string>(id => id != null && id.Length == 36)), Times.Once);
        }

        #endregion

        #region HandleMessageAsync Tests

        [Fact]
        public async Task HandleMessageAsync_CreatedEvent_SavesUser()
        {
            var userRepoMock = new Mock<IUserRepository>();
            userRepoMock.Setup(r => r.SaveOrUpdateUserAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            var familyServiceMock = new Mock<IFamilyService>();
            familyServiceMock.Setup(f => f.CreateDefaultAsync(It.IsAny<int>())).Returns(Task.CompletedTask);

            var scopeMock = new Mock<IServiceScope>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IUserRepository))).Returns(userRepoMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IFamilyService))).Returns(familyServiceMock.Object);
            scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

            var consumer = CreateConsumer();
            var message = new UserEventMessage
            {
                EventType = UserEventType.Created,
                UserId = 11,
                FirstName = "Alice",
                LastName = "Smith",
                Email = "alice@example.com",
                FamilyId = 3
            };

            await InvokeHandleMessage(consumer, message, scopeMock.Object);

            userRepoMock.Verify(r => r.SaveOrUpdateUserAsync(It.Is<User>(u =>
                u.Id == 11 && u.FirstName == "Alice" && u.LastName == "Smith"
                && u.Email == "alice@example.com" && u.FamilyId == 3)), Times.Once);
            familyServiceMock.Verify(f => f.CreateDefaultAsync(11), Times.Once);
        }

        [Fact]
        public async Task HandleMessageAsync_UpdatedEvent_SavesUser()
        {
            var userRepoMock = new Mock<IUserRepository>();
            userRepoMock.Setup(r => r.SaveOrUpdateUserAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            var scopeMock = new Mock<IServiceScope>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IUserRepository))).Returns(userRepoMock.Object);
            scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

            var consumer = CreateConsumer();
            var message = new UserEventMessage { EventType = UserEventType.Updated, UserId = 22 };

            await InvokeHandleMessage(consumer, message, scopeMock.Object);

            userRepoMock.Verify(r => r.SaveOrUpdateUserAsync(It.Is<User>(u => u.Id == 22)), Times.Once);
        }

        [Fact]
        public async Task HandleMessageAsync_DeletedEvent_ExistingUser_DeletesUser()
        {
            var existingUser = new User { Id = 33 };
            var userRepoMock = new Mock<IUserRepository>();
            userRepoMock.Setup(r => r.GetUserByIdAsync(33)).ReturnsAsync(existingUser);
            userRepoMock.Setup(r => r.DeleteUserAsync(existingUser)).Returns(Task.CompletedTask);

            var scopeMock = new Mock<IServiceScope>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IUserRepository))).Returns(userRepoMock.Object);
            scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

            var consumer = CreateConsumer();
            var message = new UserEventMessage { EventType = UserEventType.Deleted, UserId = 33 };

            await InvokeHandleMessage(consumer, message, scopeMock.Object);

            userRepoMock.Verify(r => r.DeleteUserAsync(existingUser), Times.Once);
        }

        [Fact]
        public async Task HandleMessageAsync_DeletedEvent_NonExistingUser_SkipsDelete()
        {
            var userRepoMock = new Mock<IUserRepository>();
            userRepoMock.Setup(r => r.GetUserByIdAsync(44)).ReturnsAsync((User?)null);

            var scopeMock = new Mock<IServiceScope>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IUserRepository))).Returns(userRepoMock.Object);
            scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

            var consumer = CreateConsumer();
            var message = new UserEventMessage { EventType = UserEventType.Deleted, UserId = 44 };

            await InvokeHandleMessage(consumer, message, scopeMock.Object);

            userRepoMock.Verify(r => r.DeleteUserAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task HandleMessageAsync_UnknownEventType_DoesNotCallRepository()
        {
            var userRepoMock = new Mock<IUserRepository>();

            var scopeMock = new Mock<IServiceScope>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IUserRepository))).Returns(userRepoMock.Object);
            scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);

            var consumer = CreateConsumer();
            var message = new UserEventMessage { EventType = "user.suspended", UserId = 55 };

            await InvokeHandleMessage(consumer, message, scopeMock.Object);

            userRepoMock.Verify(r => r.SaveOrUpdateUserAsync(It.IsAny<User>()), Times.Never);
            userRepoMock.Verify(r => r.DeleteUserAsync(It.IsAny<User>()), Times.Never);
        }

        #endregion

        #region UserEventMessage Tests

        [Fact]
        public void UserEventMessage_Properties_SetCorrectly()
        {
            var msg = new UserEventMessage
            {
                EventType = UserEventType.Created,
                UserId = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                FamilyId = 2
            };

            Assert.Equal(UserEventType.Created, msg.EventType);
            Assert.Equal(1, msg.UserId);
            Assert.Equal("John", msg.FirstName);
            Assert.Equal("Doe", msg.LastName);
            Assert.Equal("john@example.com", msg.Email);
            Assert.Equal(2, msg.FamilyId);
        }

        [Fact]
        public void UserEventType_Constants_HaveCorrectValues()
        {
            Assert.Equal("user.created", UserEventType.Created);
            Assert.Equal("user.updated", UserEventType.Updated);
            Assert.Equal("user.deleted", UserEventType.Deleted);
        }

        [Fact]
        public void UserEventMessage_NullableProperties_AllowNull()
        {
            var msg = new UserEventMessage
            {
                EventType = UserEventType.Deleted,
                UserId = 99,
                FirstName = null,
                LastName = null,
                Email = null,
                FamilyId = null
            };

            Assert.Null(msg.FirstName);
            Assert.Null(msg.LastName);
            Assert.Null(msg.Email);
            Assert.Null(msg.FamilyId);
        }

        #endregion
    }
}
