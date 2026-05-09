using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;
using Touir.ExpensesManager.Users.Messaging;
using Touir.ExpensesManager.Users.Messaging.Publishers;
using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Repositories.Contracts;

namespace Touir.ExpensesManager.Users.Tests.Messaging
{
    public class OutboxPublisherServiceTests
    {
        private readonly Mock<IServiceScopeFactory> _scopeFactoryMock = new();
        private readonly Mock<ILogger<OutboxPublisherService>> _loggerMock = new();

        private OutboxPublisherService CreateService()
            => new(_scopeFactoryMock.Object, _loggerMock.Object);

        private (Mock<IServiceScope>, Mock<IOutboxRepository>, Mock<IUserEventPublisher>) SetupScope()
        {
            var scopeMock = new Mock<IServiceScope>();
            var outboxRepoMock = new Mock<IOutboxRepository>();
            var publisherMock = new Mock<IUserEventPublisher>();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOutboxRepository))).Returns(outboxRepoMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IUserEventPublisher))).Returns(publisherMock.Object);
            scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);
            _scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

            return (scopeMock, outboxRepoMock, publisherMock);
        }

        private static Task InvokeProcessPending(OutboxPublisherService service)
        {
            var method = typeof(OutboxPublisherService)
                .GetMethod("ProcessPendingAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;
            return (Task)method.Invoke(service, [])!;
        }

        [Fact]
        public void Constructor_InitializesSuccessfully()
        {
            var service = CreateService();
            Assert.NotNull(service);
        }

        #region ExecuteAsync Tests

        [Fact]
        public async Task ExecuteAsync_CancelledBeforeStart_DoesNotProcess()
        {
            var service = CreateService();
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await service.StartAsync(cts.Token);

            _scopeFactoryMock.Verify(f => f.CreateScope(), Times.Never);
        }

        #endregion

        #region ProcessPendingAsync Tests

        [Fact]
        public async Task ProcessPendingAsync_NoPendingEvents_DoesNotPublish()
        {
            var (_, outboxRepoMock, publisherMock) = SetupScope();
            outboxRepoMock.Setup(r => r.GetPendingAsync(5))
                .ReturnsAsync(Array.Empty<OutboxEvent>());

            var service = CreateService();

            await InvokeProcessPending(service);

            publisherMock.Verify(p => p.PublishRaw(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task ProcessPendingAsync_SingleEvent_PublishesAndMarksPublished()
        {
            var (_, outboxRepoMock, publisherMock) = SetupScope();
            var evt = new OutboxEvent
            {
                Id = 1,
                MessageId = "msg-001",
                EventType = "user.created",
                Payload = "{\"userId\":1}",
                RetryCount = 0
            };
            outboxRepoMock.Setup(r => r.GetPendingAsync(5)).ReturnsAsync([evt]);
            outboxRepoMock.Setup(r => r.MarkPublishedAsync(1)).Returns(Task.CompletedTask);

            var service = CreateService();

            await InvokeProcessPending(service);

            publisherMock.Verify(p => p.PublishRaw("user.created", "{\"userId\":1}", "msg-001"), Times.Once);
            outboxRepoMock.Verify(r => r.MarkPublishedAsync(1), Times.Once);
            outboxRepoMock.Verify(r => r.MarkFailedAsync(It.IsAny<long>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ProcessPendingAsync_MultipleEvents_PublishesAll()
        {
            var (_, outboxRepoMock, publisherMock) = SetupScope();
            var events = new List<OutboxEvent>
            {
                new() { Id = 1, MessageId = "m1", EventType = "user.created", Payload = "{}", RetryCount = 0 },
                new() { Id = 2, MessageId = "m2", EventType = "user.updated", Payload = "{}", RetryCount = 1 },
                new() { Id = 3, MessageId = "m3", EventType = "user.deleted", Payload = "{}", RetryCount = 2 }
            };
            outboxRepoMock.Setup(r => r.GetPendingAsync(5)).ReturnsAsync(events);
            outboxRepoMock.Setup(r => r.MarkPublishedAsync(It.IsAny<long>())).Returns(Task.CompletedTask);

            var service = CreateService();

            await InvokeProcessPending(service);

            publisherMock.Verify(p => p.PublishRaw(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(3));
            outboxRepoMock.Verify(r => r.MarkPublishedAsync(It.IsAny<long>()), Times.Exactly(3));
        }

        [Fact]
        public async Task ProcessPendingAsync_PublishFails_MarksEventFailed()
        {
            var (_, outboxRepoMock, publisherMock) = SetupScope();
            var evt = new OutboxEvent
            {
                Id = 5,
                MessageId = "msg-fail",
                EventType = "user.created",
                Payload = "{}",
                RetryCount = 1
            };
            outboxRepoMock.Setup(r => r.GetPendingAsync(5)).ReturnsAsync([evt]);
            publisherMock.Setup(p => p.PublishRaw(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new InvalidOperationException("RabbitMQ connection failed"));
            outboxRepoMock.Setup(r => r.MarkFailedAsync(5, It.IsAny<string>())).Returns(Task.CompletedTask);

            var service = CreateService();

            await InvokeProcessPending(service);

            outboxRepoMock.Verify(r => r.MarkFailedAsync(5, It.Is<string>(e => e.Contains("RabbitMQ"))), Times.Once);
            outboxRepoMock.Verify(r => r.MarkPublishedAsync(It.IsAny<long>()), Times.Never);
        }

        [Fact]
        public async Task ProcessPendingAsync_MixedSuccess_PublishesSuccessfulAndFailsFailed()
        {
            var (_, outboxRepoMock, publisherMock) = SetupScope();
            var successEvt = new OutboxEvent { Id = 1, MessageId = "ok", EventType = "user.created", Payload = "{}", RetryCount = 0 };
            var failEvt = new OutboxEvent { Id = 2, MessageId = "fail", EventType = "user.deleted", Payload = "{}", RetryCount = 3 };
            outboxRepoMock.Setup(r => r.GetPendingAsync(5)).ReturnsAsync([successEvt, failEvt]);
            outboxRepoMock.Setup(r => r.MarkPublishedAsync(It.IsAny<long>())).Returns(Task.CompletedTask);
            outboxRepoMock.Setup(r => r.MarkFailedAsync(It.IsAny<long>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            publisherMock.Setup(p => p.PublishRaw("user.deleted", "{}", "fail"))
                .Throws(new Exception("publish failed"));

            var service = CreateService();

            await InvokeProcessPending(service);

            outboxRepoMock.Verify(r => r.MarkPublishedAsync(1), Times.Once);
            outboxRepoMock.Verify(r => r.MarkFailedAsync(2, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ProcessPendingAsync_UsesMaxRetries5()
        {
            var (_, outboxRepoMock, _) = SetupScope();
            outboxRepoMock.Setup(r => r.GetPendingAsync(5)).ReturnsAsync(Array.Empty<OutboxEvent>());

            var service = CreateService();

            await InvokeProcessPending(service);

            outboxRepoMock.Verify(r => r.GetPendingAsync(5), Times.Once);
        }

        [Fact]
        public async Task ProcessPendingAsync_ExceptionInGetPending_Propagates()
        {
            var (_, outboxRepoMock, _) = SetupScope();
            outboxRepoMock.Setup(r => r.GetPendingAsync(It.IsAny<int>()))
                .ThrowsAsync(new InvalidOperationException("DB unavailable"));

            var service = CreateService();

            await Assert.ThrowsAsync<InvalidOperationException>(() => InvokeProcessPending(service));
        }

        #endregion
    }
}
