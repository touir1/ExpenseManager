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

        [Fact]
        public async Task GetUnreadCountAsync_DelegatesToRepository()
        {
            var (sut, repoMock, _, _) = CreateSut();
            repoMock.Setup(r => r.GetUnreadCountAsync(5)).ReturnsAsync(3);

            var count = await sut.GetUnreadCountAsync(5);

            Assert.Equal(3, count);
            repoMock.Verify(r => r.GetUnreadCountAsync(5), Times.Once);
        }

        // ── HandleFamilyInvitationAsync ───────────────────────────────────────

        [Fact]
        public async Task HandleFamilyInvitationAsync_SendsEmail()
        {
            var (sut, _, _, emailMock) = CreateSut();
            var msg = new FamilyInvitationEventMessage
            {
                MessageId = "m1",
                EventType = FamilyEventType.InvitationRequested,
                InviteeEmail = "invitee@test.com",
                InviterName = "Alice",
                FamilyName = "The Smiths",
                InviteLink = "https://example.com/accept?token=abc"
            };

            await sut.HandleFamilyInvitationAsync(msg);

            emailMock.Verify(e => e.SendEmail(
                "invitee@test.com",
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(),
                true, It.IsAny<ICollection<string>>()), Times.Once);
        }

        [Fact]
        public async Task HandleFamilyInvitationAsync_EmailFailure_DoesNotThrow()
        {
            var (sut, _, _, emailMock) = CreateSut();
            emailMock.Setup(e => e.GetEmailTemplate(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .Throws(new Exception("smtp down"));
            var msg = new FamilyInvitationEventMessage
            {
                MessageId = "m1",
                EventType = FamilyEventType.InvitationRequested,
                InviteeEmail = "i@t.com",
                InviterName = "A",
                FamilyName = "F",
                InviteLink = "https://x.com"
            };

            var ex = await Record.ExceptionAsync(() => sut.HandleFamilyInvitationAsync(msg));

            Assert.Null(ex);
        }

        // ── HandleFamilyInvitationAcceptedAsync ───────────────────────────────

        [Fact]
        public async Task HandleFamilyInvitationAcceptedAsync_PersistsAndPushesAndEmails()
        {
            var (sut, repoMock, _, emailMock) = CreateSut();
            var msg = new FamilyInvitationAcceptedEventMessage
            {
                MessageId = "m2",
                EventType = FamilyEventType.InvitationAccepted,
                HeadUserId = 10,
                FamilyId = 5,
                FamilyName = "Family",
                AcceptorName = "Bob",
                AcceptorEmail = "bob@test.com"
            };

            await sut.HandleFamilyInvitationAcceptedAsync(msg);

            repoMock.Verify(r => r.CreateAsync(It.Is<Notification>(n => n.UserId == 10)), Times.Once);
            emailMock.Verify(e => e.SendEmail(
                "bob@test.com",
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(),
                true, It.IsAny<ICollection<string>>()), Times.Once);
        }

        [Fact]
        public async Task HandleFamilyInvitationAcceptedAsync_DbFailure_DoesNotThrow()
        {
            var (sut, repoMock, _, _) = CreateSut();
            repoMock.Setup(r => r.CreateAsync(It.IsAny<Notification>()))
                .ThrowsAsync(new Exception("db down"));
            var msg = new FamilyInvitationAcceptedEventMessage
            {
                MessageId = "m2", EventType = FamilyEventType.InvitationAccepted,
                HeadUserId = 10, FamilyId = 5, FamilyName = "F",
                AcceptorName = "B", AcceptorEmail = "b@t.com"
            };

            var ex = await Record.ExceptionAsync(() => sut.HandleFamilyInvitationAcceptedAsync(msg));

            Assert.Null(ex);
        }

        [Fact]
        public async Task HandleFamilyInvitationAcceptedAsync_PushFailure_DoesNotThrow()
        {
            var (sut, _, hubMock, _) = CreateSut();
            var clientsMock = new Mock<IHubClients>();
            var clientProxyMock = new Mock<IClientProxy>();
            hubMock.Setup(h => h.Clients).Returns(clientsMock.Object);
            clientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(clientProxyMock.Object);
            clientProxyMock
                .Setup(c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default))
                .ThrowsAsync(new InvalidOperationException("hub down"));
            var msg = new FamilyInvitationAcceptedEventMessage
            {
                MessageId = "m2", EventType = FamilyEventType.InvitationAccepted,
                HeadUserId = 10, FamilyId = 5, FamilyName = "F",
                AcceptorName = "B", AcceptorEmail = "b@t.com"
            };

            var ex = await Record.ExceptionAsync(() => sut.HandleFamilyInvitationAcceptedAsync(msg));

            Assert.Null(ex);
        }

        [Fact]
        public async Task HandleFamilyInvitationAcceptedAsync_EmailFailure_DoesNotThrow()
        {
            var (sut, _, _, emailMock) = CreateSut();
            emailMock.Setup(e => e.GetEmailTemplate(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .Throws(new Exception("smtp fail"));
            var msg = new FamilyInvitationAcceptedEventMessage
            {
                MessageId = "m2", EventType = FamilyEventType.InvitationAccepted,
                HeadUserId = 10, FamilyId = 5, FamilyName = "F",
                AcceptorName = "B", AcceptorEmail = "b@t.com"
            };

            var ex = await Record.ExceptionAsync(() => sut.HandleFamilyInvitationAcceptedAsync(msg));

            Assert.Null(ex);
        }

        // ── HandleFamilyMemberJoinedAsync ─────────────────────────────────────

        [Fact]
        public async Task HandleFamilyMemberJoinedAsync_PersistsForEachMember()
        {
            var (sut, repoMock, _, _) = CreateSut();
            var msg = new FamilyMemberJoinedEventMessage
            {
                MessageId = "m3", EventType = FamilyEventType.MemberJoined,
                FamilyId = 5, FamilyName = "F",
                JoinerName = "Carol", JoinerUserId = 20,
                MemberUserIds = [1, 2, 3]
            };

            await sut.HandleFamilyMemberJoinedAsync(msg);

            repoMock.Verify(r => r.CreateAsync(It.IsAny<Notification>()), Times.Exactly(3));
        }

        [Fact]
        public async Task HandleFamilyMemberJoinedAsync_EmptyMembers_NoPersist()
        {
            var (sut, repoMock, _, _) = CreateSut();
            var msg = new FamilyMemberJoinedEventMessage
            {
                MessageId = "m3", EventType = FamilyEventType.MemberJoined,
                FamilyId = 5, FamilyName = "F",
                JoinerName = "Carol", JoinerUserId = 20,
                MemberUserIds = []
            };

            await sut.HandleFamilyMemberJoinedAsync(msg);

            repoMock.Verify(r => r.CreateAsync(It.IsAny<Notification>()), Times.Never);
        }

        [Fact]
        public async Task HandleFamilyMemberJoinedAsync_DbFailurePerMember_DoesNotThrow()
        {
            var (sut, repoMock, _, _) = CreateSut();
            repoMock.Setup(r => r.CreateAsync(It.IsAny<Notification>()))
                .ThrowsAsync(new Exception("db down"));
            var msg = new FamilyMemberJoinedEventMessage
            {
                MessageId = "m3", EventType = FamilyEventType.MemberJoined,
                FamilyId = 5, FamilyName = "F",
                JoinerName = "Carol", JoinerUserId = 20,
                MemberUserIds = [1, 2]
            };

            var ex = await Record.ExceptionAsync(() => sut.HandleFamilyMemberJoinedAsync(msg));

            Assert.Null(ex);
        }

        [Fact]
        public async Task HandleFamilyMemberJoinedAsync_PushFailure_DoesNotThrow()
        {
            var (sut, _, hubMock, _) = CreateSut();
            var clientsMock = new Mock<IHubClients>();
            var clientProxyMock = new Mock<IClientProxy>();
            hubMock.Setup(h => h.Clients).Returns(clientsMock.Object);
            clientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(clientProxyMock.Object);
            clientProxyMock
                .Setup(c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default))
                .ThrowsAsync(new InvalidOperationException("hub down"));
            var msg = new FamilyMemberJoinedEventMessage
            {
                MessageId = "m3", EventType = FamilyEventType.MemberJoined,
                FamilyId = 5, FamilyName = "F",
                JoinerName = "Carol", JoinerUserId = 20,
                MemberUserIds = [1]
            };

            var ex = await Record.ExceptionAsync(() => sut.HandleFamilyMemberJoinedAsync(msg));

            Assert.Null(ex);
        }

        // ── HandleFamilyExpenseAddedAsync / DeletedAsync ──────────────────────

        [Fact]
        public async Task HandleFamilyExpenseAddedAsync_PersistsForEachMember()
        {
            var (sut, repoMock, _, _) = CreateSut();
            var msg = new FamilyExpenseEventMessage
            {
                MessageId = "m4", EventType = FamilyEventType.ExpenseAdded,
                FamilyId = 5, FamilyName = "F",
                ExpenseId = 100, Amount = 50m, CurrencyCode = "EUR",
                ActorName = "Alice", ActorUserId = 1,
                MemberUserIds = [2, 3]
            };

            await sut.HandleFamilyExpenseAddedAsync(msg);

            repoMock.Verify(r => r.CreateAsync(It.IsAny<Notification>()), Times.Exactly(2));
        }

        [Fact]
        public async Task HandleFamilyExpenseDeletedAsync_PersistsForEachMember()
        {
            var (sut, repoMock, _, _) = CreateSut();
            var msg = new FamilyExpenseEventMessage
            {
                MessageId = "m5", EventType = FamilyEventType.ExpenseDeleted,
                FamilyId = 5, FamilyName = "F",
                ExpenseId = 100, Amount = 50m, CurrencyCode = "EUR",
                ActorName = "Alice", ActorUserId = 1,
                MemberUserIds = [2]
            };

            await sut.HandleFamilyExpenseDeletedAsync(msg);

            repoMock.Verify(r => r.CreateAsync(It.IsAny<Notification>()), Times.Once);
        }

        [Fact]
        public async Task HandleFamilyExpenseAddedAsync_DbFailure_DoesNotThrow()
        {
            var (sut, repoMock, _, _) = CreateSut();
            repoMock.Setup(r => r.CreateAsync(It.IsAny<Notification>()))
                .ThrowsAsync(new Exception("db down"));
            var msg = new FamilyExpenseEventMessage
            {
                MessageId = "m4", EventType = FamilyEventType.ExpenseAdded,
                FamilyId = 5, FamilyName = "F",
                ExpenseId = 100, Amount = 50m, CurrencyCode = "EUR",
                ActorName = "Alice", ActorUserId = 1,
                MemberUserIds = [2]
            };

            var ex = await Record.ExceptionAsync(() => sut.HandleFamilyExpenseAddedAsync(msg));

            Assert.Null(ex);
        }

        [Fact]
        public async Task HandleFamilyExpenseAddedAsync_PushFailure_DoesNotThrow()
        {
            var (sut, _, hubMock, _) = CreateSut();
            var clientsMock = new Mock<IHubClients>();
            var clientProxyMock = new Mock<IClientProxy>();
            hubMock.Setup(h => h.Clients).Returns(clientsMock.Object);
            clientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(clientProxyMock.Object);
            clientProxyMock
                .Setup(c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default))
                .ThrowsAsync(new InvalidOperationException("hub down"));
            var msg = new FamilyExpenseEventMessage
            {
                MessageId = "m4", EventType = FamilyEventType.ExpenseAdded,
                FamilyId = 5, FamilyName = "F",
                ExpenseId = 100, Amount = 50m, CurrencyCode = "EUR",
                ActorName = "Alice", ActorUserId = 1,
                MemberUserIds = [2]
            };

            var ex = await Record.ExceptionAsync(() => sut.HandleFamilyExpenseAddedAsync(msg));

            Assert.Null(ex);
        }

        // ── HandleImportCompletedAsync ────────────────────────────────────────

        [Fact]
        public async Task HandleImportCompletedAsync_PersistsAndPushes()
        {
            var (sut, repoMock, _, _) = CreateSut();
            var msg = new ImportCompletedEventMessage
            {
                MessageId = "m6", EventType = FamilyEventType.ImportCompleted,
                UserId = 7, TotalRows = 100, ImportedCount = 98, SkippedCount = 2
            };

            await sut.HandleImportCompletedAsync(msg);

            repoMock.Verify(r => r.CreateAsync(It.Is<Notification>(n => n.UserId == 7)), Times.Once);
        }

        [Fact]
        public async Task HandleImportCompletedAsync_DbFailure_DoesNotThrow()
        {
            var (sut, repoMock, _, _) = CreateSut();
            repoMock.Setup(r => r.CreateAsync(It.IsAny<Notification>()))
                .ThrowsAsync(new Exception("db down"));
            var msg = new ImportCompletedEventMessage
            {
                MessageId = "m6", EventType = FamilyEventType.ImportCompleted,
                UserId = 7, TotalRows = 10, ImportedCount = 10, SkippedCount = 0
            };

            var ex = await Record.ExceptionAsync(() => sut.HandleImportCompletedAsync(msg));

            Assert.Null(ex);
        }

        [Fact]
        public async Task HandleImportCompletedAsync_PushFailure_DoesNotThrow()
        {
            var (sut, _, hubMock, _) = CreateSut();
            var clientsMock = new Mock<IHubClients>();
            var clientProxyMock = new Mock<IClientProxy>();
            hubMock.Setup(h => h.Clients).Returns(clientsMock.Object);
            clientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(clientProxyMock.Object);
            clientProxyMock
                .Setup(c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default))
                .ThrowsAsync(new InvalidOperationException("hub down"));
            var msg = new ImportCompletedEventMessage
            {
                MessageId = "m6", EventType = FamilyEventType.ImportCompleted,
                UserId = 7, TotalRows = 10, ImportedCount = 10, SkippedCount = 0
            };

            var ex = await Record.ExceptionAsync(() => sut.HandleImportCompletedAsync(msg));

            Assert.Null(ex);
        }

        // ── HandleRateConflictAsync ───────────────────────────────────────────

        [Fact]
        public async Task HandleRateConflictAsync_PersistsForEachAdmin()
        {
            var (sut, repoMock, _, _) = CreateSut();
            var msg = new RateConflictEventMessage
            {
                MessageId = "m7", EventType = FamilyEventType.RateConflict,
                ConflictId = 1, SourceCurrencyCode = "USD", DestCurrencyCode = "EUR",
                Date = new DateOnly(2024, 1, 1), AutoRate = 0.9m, ManualRate = 0.85m,
                AdminUserIds = [10, 11]
            };

            await sut.HandleRateConflictAsync(msg);

            repoMock.Verify(r => r.CreateAsync(It.IsAny<Notification>()), Times.Exactly(2));
        }

        [Fact]
        public async Task HandleRateConflictAsync_DbFailure_DoesNotThrow()
        {
            var (sut, repoMock, _, _) = CreateSut();
            repoMock.Setup(r => r.CreateAsync(It.IsAny<Notification>()))
                .ThrowsAsync(new Exception("db down"));
            var msg = new RateConflictEventMessage
            {
                MessageId = "m7", EventType = FamilyEventType.RateConflict,
                ConflictId = 1, SourceCurrencyCode = "USD", DestCurrencyCode = "EUR",
                Date = new DateOnly(2024, 1, 1), AutoRate = 0.9m, ManualRate = 0.85m,
                AdminUserIds = [10]
            };

            var ex = await Record.ExceptionAsync(() => sut.HandleRateConflictAsync(msg));

            Assert.Null(ex);
        }

        [Fact]
        public async Task HandleRateConflictAsync_PushFailure_DoesNotThrow()
        {
            var (sut, _, hubMock, _) = CreateSut();
            var clientsMock = new Mock<IHubClients>();
            var clientProxyMock = new Mock<IClientProxy>();
            hubMock.Setup(h => h.Clients).Returns(clientsMock.Object);
            clientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(clientProxyMock.Object);
            clientProxyMock
                .Setup(c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default))
                .ThrowsAsync(new InvalidOperationException("hub down"));
            var msg = new RateConflictEventMessage
            {
                MessageId = "m7", EventType = FamilyEventType.RateConflict,
                ConflictId = 1, SourceCurrencyCode = "USD", DestCurrencyCode = "EUR",
                Date = new DateOnly(2024, 1, 1), AutoRate = 0.9m, ManualRate = 0.85m,
                AdminUserIds = [10]
            };

            var ex = await Record.ExceptionAsync(() => sut.HandleRateConflictAsync(msg));

            Assert.Null(ex);
        }

        // ── HandleEmailVerificationAsync ──────────────────────────────────────

        [Fact]
        public async Task HandleEmailVerificationAsync_SendsEmail()
        {
            var (sut, _, _, emailMock) = CreateSut();
            var msg = new UserNotificationEventMessage
            {
                MessageId = "m8", EventType = UserEventType.EmailVerificationRequested,
                UserId = 1, Email = "user@test.com",
                VerificationLink = "https://x.com/verify?token=abc"
            };

            await sut.HandleEmailVerificationAsync(msg);

            emailMock.Verify(e => e.SendEmail(
                "user@test.com",
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(),
                true, It.IsAny<ICollection<string>>()), Times.Once);
        }

        [Fact]
        public async Task HandleEmailVerificationAsync_EmailFailure_DoesNotThrow()
        {
            var (sut, _, _, emailMock) = CreateSut();
            emailMock.Setup(e => e.GetEmailTemplate(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .Throws(new Exception("smtp fail"));
            var msg = new UserNotificationEventMessage
            {
                MessageId = "m8", EventType = UserEventType.EmailVerificationRequested,
                UserId = 1, Email = "u@t.com", VerificationLink = "https://x.com"
            };

            var ex = await Record.ExceptionAsync(() => sut.HandleEmailVerificationAsync(msg));

            Assert.Null(ex);
        }

        // ── HandlePasswordResetAsync ──────────────────────────────────────────

        [Fact]
        public async Task HandlePasswordResetAsync_SendsEmail()
        {
            var (sut, _, _, emailMock) = CreateSut();
            var msg = new UserNotificationEventMessage
            {
                MessageId = "m9", EventType = UserEventType.PasswordResetRequested,
                UserId = 1, Email = "user@test.com",
                ResetLink = "https://x.com/reset?token=xyz"
            };

            await sut.HandlePasswordResetAsync(msg);

            emailMock.Verify(e => e.SendEmail(
                "user@test.com",
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(),
                true, It.IsAny<ICollection<string>>()), Times.Once);
        }

        [Fact]
        public async Task HandlePasswordResetAsync_EmailFailure_DoesNotThrow()
        {
            var (sut, _, _, emailMock) = CreateSut();
            emailMock.Setup(e => e.GetEmailTemplate(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .Throws(new Exception("smtp fail"));
            var msg = new UserNotificationEventMessage
            {
                MessageId = "m9", EventType = UserEventType.PasswordResetRequested,
                UserId = 1, Email = "u@t.com", ResetLink = "https://x.com"
            };

            var ex = await Record.ExceptionAsync(() => sut.HandlePasswordResetAsync(msg));

            Assert.Null(ex);
        }

        // ── HandlePasswordChangedAsync ────────────────────────────────────────

        [Fact]
        public async Task HandlePasswordChangedAsync_SendsEmail()
        {
            var (sut, _, _, emailMock) = CreateSut();
            var msg = new UserNotificationEventMessage
            {
                MessageId = "m10", EventType = UserEventType.PasswordChanged,
                UserId = 1, Email = "user@test.com", FirstName = "Alice"
            };

            await sut.HandlePasswordChangedAsync(msg);

            emailMock.Verify(e => e.SendEmail(
                "user@test.com",
                It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(),
                true, It.IsAny<ICollection<string>>()), Times.Once);
        }

        [Fact]
        public async Task HandlePasswordChangedAsync_EmailFailure_DoesNotThrow()
        {
            var (sut, _, _, emailMock) = CreateSut();
            emailMock.Setup(e => e.GetEmailTemplate(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .Throws(new Exception("smtp fail"));
            var msg = new UserNotificationEventMessage
            {
                MessageId = "m10", EventType = UserEventType.PasswordChanged,
                UserId = 1, Email = "u@t.com", FirstName = "A"
            };

            var ex = await Record.ExceptionAsync(() => sut.HandlePasswordChangedAsync(msg));

            Assert.Null(ex);
        }
    }
}
