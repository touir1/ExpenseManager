using Moq;
using RabbitMQ.Client;
using Touir.ExpensesManager.Users.Messaging.Messages;
using Touir.ExpensesManager.Users.Messaging.Publishers;
using Touir.ExpensesManager.Users.Services.Contracts;

namespace Touir.ExpensesManager.Users.Tests.Messaging
{
    public class UserEventPublisherTests
    {
        private readonly Mock<IRabbitMQService> _rabbitMqServiceMock = new();
        private readonly Mock<IConnection> _connectionMock = new();
        private readonly Mock<IModel> _channelMock = new();
        private readonly Mock<IBasicProperties> _propsMock = new();

        public UserEventPublisherTests()
        {
            _connectionMock.Setup(c => c.CreateModel()).Returns(_channelMock.Object);
            _rabbitMqServiceMock.Setup(r => r.GetConnection()).Returns(_connectionMock.Object);
            _channelMock.Setup(c => c.CreateBasicProperties()).Returns(_propsMock.Object);
        }

        private UserEventPublisher CreatePublisher()
            => new(_rabbitMqServiceMock.Object);

        [Fact]
        public void Constructor_InitializesSuccessfully()
        {
            var publisher = CreatePublisher();
            Assert.NotNull(publisher);
        }

        #region Publish Tests

        [Fact]
        public void Publish_CallsPublishRaw_WithSerializedJson()
        {
            var publisher = CreatePublisher();
            var message = new UserEventMessage
            {
                EventType = UserEventType.Created,
                UserId = 1,
                FirstName = "Alice",
                LastName = "Smith",
                Email = "alice@example.com"
            };

            publisher.Publish(message);

            _channelMock.Verify(c => c.BasicPublish(
                "users.events",
                UserEventType.Created,
                false,
                _propsMock.Object,
                It.Is<ReadOnlyMemory<byte>>(b => b.Length > 0)),
                Times.Once);
        }

        [Fact]
        public void Publish_SetsMessageProperties_BeforePublishing()
        {
            var publisher = CreatePublisher();
            var message = new UserEventMessage
            {
                EventType = UserEventType.Updated,
                UserId = 2
            };

            publisher.Publish(message);

            _propsMock.VerifySet(p => p.Persistent = true, Times.Once);
            _propsMock.VerifySet(p => p.ContentType = "application/json", Times.Once);
        }

        [Fact]
        public void Publish_SetsUniqueMessageId_EachCall()
        {
            var publisher = CreatePublisher();
            var message = new UserEventMessage { EventType = UserEventType.Created, UserId = 1 };

            string? firstId = null;
            string? secondId = null;

            _propsMock.SetupSet(p => p.MessageId = It.IsAny<string>())
                .Callback<string>(id =>
                {
                    if (firstId == null) firstId = id;
                    else secondId = id;
                });

            publisher.Publish(message);
            publisher.Publish(message);

            Assert.NotNull(firstId);
            Assert.NotNull(secondId);
            Assert.NotEqual(firstId, secondId);
        }

        [Fact]
        public void Publish_CreatesNewChannel_PerCall()
        {
            var publisher = CreatePublisher();
            var message = new UserEventMessage { EventType = UserEventType.Created, UserId = 1 };

            publisher.Publish(message);
            publisher.Publish(message);

            _connectionMock.Verify(c => c.CreateModel(), Times.Exactly(2));
        }

        [Fact]
        public void Publish_DisposesChannel_AfterPublishing()
        {
            var publisher = CreatePublisher();
            var message = new UserEventMessage { EventType = UserEventType.Deleted, UserId = 5 };

            publisher.Publish(message);

            _channelMock.Verify(c => c.Dispose(), Times.Once);
        }

        #endregion

        #region PublishRaw Tests

        [Fact]
        public void PublishRaw_PublishesToCorrectExchange_WithRoutingKey()
        {
            var publisher = CreatePublisher();

            publisher.PublishRaw(UserEventType.Created, "{\"userId\":1}", "msg-id-123");

            _channelMock.Verify(c => c.BasicPublish(
                "users.events",
                UserEventType.Created,
                false,
                _propsMock.Object,
                It.Is<ReadOnlyMemory<byte>>(b => b.Length > 0)),
                Times.Once);
        }

        [Fact]
        public void PublishRaw_SetsMessageIdFromArgument()
        {
            var publisher = CreatePublisher();
            var messageId = "fixed-message-id";

            publisher.PublishRaw(UserEventType.Updated, "{}", messageId);

            _propsMock.VerifySet(p => p.MessageId = messageId, Times.Once);
        }

        [Fact]
        public void PublishRaw_SetsPersistentAndContentType()
        {
            var publisher = CreatePublisher();

            publisher.PublishRaw(UserEventType.Deleted, "{\"userId\":3}", "msg-456");

            _propsMock.VerifySet(p => p.Persistent = true, Times.Once);
            _propsMock.VerifySet(p => p.ContentType = "application/json", Times.Once);
        }

        [Fact]
        public void PublishRaw_EncodesPayloadAsUtf8()
        {
            var publisher = CreatePublisher();
            var payload = "{\"userId\":42,\"name\":\"Test\"}";

            ReadOnlyMemory<byte> publishedBody = default;
            _channelMock.Setup(c => c.BasicPublish(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<IBasicProperties>(), It.IsAny<ReadOnlyMemory<byte>>()))
                .Callback<string, string, bool, IBasicProperties, ReadOnlyMemory<byte>>(
                    (_, _, _, _, body) => publishedBody = body);

            publisher.PublishRaw(UserEventType.Created, payload, "test-id");

            var publishedString = System.Text.Encoding.UTF8.GetString(publishedBody.ToArray());
            Assert.Equal(payload, publishedString);
        }

        [Fact]
        public void PublishRaw_DeclaresExchange_BeforePublishing()
        {
            var publisher = CreatePublisher();

            publisher.PublishRaw(UserEventType.Created, "{}", "msg-789");

            _channelMock.Verify(c => c.ExchangeDeclare(
                "users.events", ExchangeType.Topic, true, false,
                It.IsAny<IDictionary<string, object>>()),
                Times.Once);
        }

        [Fact]
        public void PublishRaw_CreatesAndDisposesChannel()
        {
            var publisher = CreatePublisher();

            publisher.PublishRaw(UserEventType.Deleted, "{\"userId\":5}", "msg-abc");

            _connectionMock.Verify(c => c.CreateModel(), Times.Once);
            _channelMock.Verify(c => c.Dispose(), Times.Once);
        }

        [Theory]
        [InlineData(UserEventType.Created)]
        [InlineData(UserEventType.Updated)]
        [InlineData(UserEventType.Deleted)]
        public void PublishRaw_SupportsAllEventTypes(string eventType)
        {
            var publisher = CreatePublisher();

            publisher.PublishRaw(eventType, "{\"userId\":1}", "msg-" + eventType);

            _channelMock.Verify(c => c.BasicPublish(
                "users.events", eventType, false, _propsMock.Object,
                It.IsAny<ReadOnlyMemory<byte>>()),
                Times.Once);
        }

        #endregion
    }
}
