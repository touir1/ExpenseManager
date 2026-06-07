using Moq;
using RabbitMQ.Client;
using Touir.ExpensesManager.Expenses.Messaging.Messages;
using Touir.ExpensesManager.Expenses.Messaging.Publishers;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Tests.Messaging
{
    public class FamilyEventPublisherTests
    {
        private static (FamilyEventPublisher Publisher, Mock<IModel> Channel) Create()
        {
            var channel = new Mock<IModel>();
            var props = new Mock<IBasicProperties>();
            channel.Setup(c => c.CreateBasicProperties()).Returns(props.Object);

            var connection = new Mock<IConnection>();
            connection.Setup(c => c.CreateModel()).Returns(channel.Object);

            var rabbitMq = new Mock<IRabbitMQService>();
            rabbitMq.Setup(r => r.GetConnection()).Returns(connection.Object);

            return (new FamilyEventPublisher(rabbitMq.Object), channel);
        }

        [Fact]
        public void PublishRaw_CallsExchangeDeclareAndBasicPublish()
        {
            var (publisher, channel) = Create();

            publisher.PublishRaw("family.member.removed", "{}", "msg-1");

            channel.Verify(c => c.ExchangeDeclare(
                "expenses.events",
                ExchangeType.Topic,
                true, false, null), Times.Once);

            channel.Verify(c => c.BasicPublish(
                "expenses.events",
                "family.member.removed",
                false,
                It.IsAny<IBasicProperties>(),
                It.IsAny<ReadOnlyMemory<byte>>()), Times.Once);
        }

        [Fact]
        public void Publish_SerializesAndCallsPublishRaw()
        {
            var (publisher, channel) = Create();

            var message = new FamilyEventMessage
            {
                MessageId = "msg-2",
                EventType = "family.member.removed",
                FamilyId = 1,
                FamilyName = "Test",
                TargetUserId = 5,
                TargetEmail = "a@b.com",
                TargetFirstName = "Alice",
                RemovedByUserId = 10,
                RemovedByName = "Bob",
                OccurredAt = DateTime.UtcNow
            };

            publisher.Publish(message);

            channel.Verify(c => c.BasicPublish(
                "expenses.events",
                "family.member.removed",
                false,
                It.IsAny<IBasicProperties>(),
                It.IsAny<ReadOnlyMemory<byte>>()), Times.Once);
        }

        [Fact]
        public void PublishRaw_SetsMessageIdOnProperties()
        {
            var channel = new Mock<IModel>();
            var props = new Mock<IBasicProperties>();
            channel.Setup(c => c.CreateBasicProperties()).Returns(props.Object);

            var connection = new Mock<IConnection>();
            connection.Setup(c => c.CreateModel()).Returns(channel.Object);
            var rabbitMq = new Mock<IRabbitMQService>();
            rabbitMq.Setup(r => r.GetConnection()).Returns(connection.Object);

            new FamilyEventPublisher(rabbitMq.Object).PublishRaw("test.event", "{}", "my-msg-id");

            props.VerifySet(p => p.MessageId = "my-msg-id", Times.Once);
            props.VerifySet(p => p.Persistent = true, Times.Once);
        }
    }
}
