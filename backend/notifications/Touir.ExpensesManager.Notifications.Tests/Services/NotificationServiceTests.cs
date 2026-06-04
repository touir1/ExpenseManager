using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Text.Json;
using Touir.ExpensesManager.Notifications.Hubs;
using Touir.ExpensesManager.Notifications.Infrastructure.Contracts;
using Touir.ExpensesManager.Notifications.Messaging.Messages;
using Touir.ExpensesManager.Notifications.Models;
using Touir.ExpensesManager.Notifications.Repositories.Contracts;
using Touir.ExpensesManager.Notifications.Services;

namespace Touir.ExpensesManager.Notifications.Tests.Services
{
    public class NotificationServiceTests
    {
        private static FamilyEventMessage MakeMessage() => new()
        {
            MessageId = Guid.NewGuid().ToString(),
            EventType = FamilyEventType.MemberRemoved,
            TargetUserId = 1,
            TargetEmail = "user@test.com",
            TargetFirstName = "Alice",
            FamilyId = 10,
            FamilyName = "Test Family",
            RemovedByUserId = 2,
            RemovedByName = "Bob Smith",
            ExpenseCount = 3,
            OccurredAt = DateTime.UtcNow
        };

        private static (NotificationService sut,
            Mock<INotificationRepository> repoMock,
            Mock<IHubContext<NotificationHub>> hubMock,
            Mock<IEmailHelper> emailMock)
            CreateSut()
        {
            var repoMock = new Mock<INotificationRepository>();
            var hubMock = new Mock<IHubContext<NotificationHub>>();
            var emailMock = new Mock<IEmailHelper>();

            repoMock
                .Setup(r => r.CreateAsync(It.IsAny<Notification>()))
                .ReturnsAsync((Notification n) => { n.Id = 1; return n; });

            var clientsMock = new Mock<IHubClients>();
            var clientProxyMock = new Mock<IClientProxy>();
            hubMock.Setup(h => h.Clients).Returns(clientsMock.Object);
            clientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(clientProxyMock.Object);

            emailMock
                .Setup(e => e.GetEmailTemplate(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .Returns("<html>email</html>");

            var sut = new NotificationService(
                repoMock.Object,
                hubMock.Object,
                emailMock.Object,
                NullLogger<NotificationService>.Instance);

            return (sut, repoMock, hubMock, emailMock);
        }

        [Fact]
        public async Task HandleFamilyMemberRemovedAsync_PersistsBeforePush()
        {
            var (sut, repoMock, hubMock, _) = CreateSut();
            var callOrder = new List<string>();

            repoMock
                .Setup(r => r.CreateAsync(It.IsAny<Notification>()))
                .Callback(() => callOrder.Add("persist"))
                .ReturnsAsync((Notification n) => { n.Id = 1; return n; });

            var clientsMock = new Mock<IHubClients>();
            var clientProxyMock = new Mock<IClientProxy>();
            hubMock.Setup(h => h.Clients).Returns(clientsMock.Object);
            clientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(clientProxyMock.Object);
            clientProxyMock
                .Setup(c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default))
                .Callback(() => callOrder.Add("push"))
                .Returns(Task.CompletedTask);

            await sut.HandleFamilyMemberRemovedAsync(MakeMessage());

            Assert.Equal("persist", callOrder[0]);
            Assert.Equal("push", callOrder[1]);
        }

        [Fact]
        public async Task HandleFamilyMemberRemovedAsync_PushFailure_DoesNotThrow()
        {
            var (sut, _, hubMock, _) = CreateSut();

            var clientsMock = new Mock<IHubClients>();
            var clientProxyMock = new Mock<IClientProxy>();
            hubMock.Setup(h => h.Clients).Returns(clientsMock.Object);
            clientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(clientProxyMock.Object);
            clientProxyMock
                .Setup(c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default))
                .ThrowsAsync(new InvalidOperationException("hub down"));

            var ex = await Record.ExceptionAsync(() => sut.HandleFamilyMemberRemovedAsync(MakeMessage()));
            Assert.Null(ex);
        }

        [Fact]
        public async Task HandleFamilyMemberRemovedAsync_EmailFailure_DoesNotThrow()
        {
            var (sut, _, _, emailMock) = CreateSut();

            emailMock
                .Setup(e => e.GetEmailTemplate(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .Throws(new Exception("smtp down"));

            var ex = await Record.ExceptionAsync(() => sut.HandleFamilyMemberRemovedAsync(MakeMessage()));
            Assert.Null(ex);
        }

        [Fact]
        public async Task HandleFamilyMemberRemovedAsync_StoresCorrectPayload()
        {
            var (sut, repoMock, _, _) = CreateSut();
            Notification? captured = null;
            repoMock
                .Setup(r => r.CreateAsync(It.IsAny<Notification>()))
                .Callback<Notification>(n => captured = n)
                .ReturnsAsync((Notification n) => { n.Id = 1; return n; });

            await sut.HandleFamilyMemberRemovedAsync(MakeMessage());

            Assert.NotNull(captured);
            Assert.Equal(NotificationType.FamilyMemberRemoved, captured!.Type);
            Assert.Equal(1, captured.UserId);
            Assert.False(captured.IsRead);
            var payload = JsonSerializer.Deserialize<JsonElement>(captured.Payload);
            Assert.Equal("FAMILY_MEMBER_REMOVED", payload.GetProperty("type").GetString());
            Assert.Equal("Test Family", payload.GetProperty("familyName").GetString());
        }

        [Fact]
        public async Task HandleFamilyMemberRemovedAsync_PushesTo_CorrectGroup()
        {
            var (sut, _, hubMock, _) = CreateSut();
            var capturedGroup = "";
            var clientsMock = new Mock<IHubClients>();
            var clientProxyMock = new Mock<IClientProxy>();
            hubMock.Setup(h => h.Clients).Returns(clientsMock.Object);
            clientsMock
                .Setup(c => c.Group(It.IsAny<string>()))
                .Callback<string>(g => capturedGroup = g)
                .Returns(clientProxyMock.Object);

            await sut.HandleFamilyMemberRemovedAsync(MakeMessage());

            Assert.Equal("1", capturedGroup);
        }

        [Fact]
        public async Task HandleFamilyMemberRemovedAsync_SendsEmail_ToTargetAddress()
        {
            var (sut, _, _, emailMock) = CreateSut();

            await sut.HandleFamilyMemberRemovedAsync(MakeMessage());

            emailMock.Verify(e => e.SendEmail(
                "user@test.com",
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(),
                true, It.IsAny<ICollection<string>>()), Times.Once);
        }

        [Fact]
        public async Task GetNotificationsAsync_DelegatesToRepository()
        {
            var (sut, repoMock, _, _) = CreateSut();
            repoMock
                .Setup(r => r.GetByUserAsync(5, 1, 20))
                .ReturnsAsync(new List<Notification>());

            await sut.GetNotificationsAsync(5, 1, 20);

            repoMock.Verify(r => r.GetByUserAsync(5, 1, 20), Times.Once);
        }

        [Fact]
        public async Task MarkAsReadAsync_DelegatesToRepository()
        {
            var (sut, repoMock, _, _) = CreateSut();

            await sut.MarkAsReadAsync(42, 7);

            repoMock.Verify(r => r.MarkAsReadAsync(42, 7), Times.Once);
        }

        [Fact]
        public async Task MarkAllAsReadAsync_DelegatesToRepository()
        {
            var (sut, repoMock, _, _) = CreateSut();

            await sut.MarkAllAsReadAsync(7);

            repoMock.Verify(r => r.MarkAllAsReadAsync(7), Times.Once);
        }
    }
}
