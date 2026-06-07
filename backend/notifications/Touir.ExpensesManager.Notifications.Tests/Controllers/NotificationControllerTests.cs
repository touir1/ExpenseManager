using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Touir.ExpensesManager.Notifications.Controllers;
using Touir.ExpensesManager.Notifications.Controllers.Responses;
using Touir.ExpensesManager.Notifications.Models;
using Touir.ExpensesManager.Notifications.Services.Contracts;

namespace Touir.ExpensesManager.Notifications.Tests.Controllers
{
    public class NotificationControllerTests
    {
        // JWT with sub=42
        private const string FakeJwt =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9" +
            ".eyJzdWIiOiI0MiIsImV4cCI6OTk5OTk5OTk5OX0" +
            ".placeholder";

        private static NotificationController CreateController(
            INotificationService? service = null,
            string? jwtCookie = FakeJwt)
        {
            var controller = new NotificationController(service ?? Mock.Of<INotificationService>());
            var httpContext = new DefaultHttpContext();
            if (jwtCookie is not null)
                httpContext.Request.Headers.Cookie = $"auth_token={jwtCookie}";
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            return controller;
        }

        private static Notification MakeNotification(long id = 1) => new()
        {
            Id = id,
            UserId = 42,
            Type = NotificationType.FamilyMemberRemoved,
            Payload = """{"type":"FAMILY_MEMBER_REMOVED"}""",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        // ── GetNotifications ──────────────────────────────────────────────────

        [Fact]
        public async Task GetNotifications_Returns200_WithDtos()
        {
            var svcMock = new Mock<INotificationService>();
            svcMock.Setup(s => s.GetNotificationsAsync(42, 1, 20))
                .ReturnsAsync(new List<Notification> { MakeNotification() });
            var controller = CreateController(svcMock.Object);

            var result = await controller.GetNotifications();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
        }

        [Fact]
        public async Task GetNotifications_Returns401_WhenNoCookie()
        {
            var controller = CreateController(jwtCookie: null);

            var result = await controller.GetNotifications();

            var unauth = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("UNAUTHORIZED", ((ErrorResponse)unauth.Value!).Message);
        }

        [Fact]
        public async Task GetNotifications_Returns400_WhenServiceThrows()
        {
            var svcMock = new Mock<INotificationService>();
            svcMock.Setup(s => s.GetNotificationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("db down"));
            var controller = CreateController(svcMock.Object);

            var result = await controller.GetNotifications();

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("SERVER_ERROR", ((ErrorResponse)bad.Value!).Message);
        }

        // ── GetUnreadCount ────────────────────────────────────────────────────

        [Fact]
        public async Task GetUnreadCount_Returns200_WithCount()
        {
            var svcMock = new Mock<INotificationService>();
            svcMock.Setup(s => s.GetUnreadCountAsync(42)).ReturnsAsync(5);
            var controller = CreateController(svcMock.Object);

            var result = await controller.GetUnreadCount();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
        }

        [Fact]
        public async Task GetUnreadCount_Returns401_WhenNoCookie()
        {
            var controller = CreateController(jwtCookie: null);

            var result = await controller.GetUnreadCount();

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task GetUnreadCount_Returns400_WhenServiceThrows()
        {
            var svcMock = new Mock<INotificationService>();
            svcMock.Setup(s => s.GetUnreadCountAsync(It.IsAny<int>()))
                .ThrowsAsync(new Exception("fail"));
            var controller = CreateController(svcMock.Object);

            var result = await controller.GetUnreadCount();

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // ── MarkAsRead ────────────────────────────────────────────────────────

        [Fact]
        public async Task MarkAsRead_Returns204_OnSuccess()
        {
            var svcMock = new Mock<INotificationService>();
            svcMock.Setup(s => s.MarkAsReadAsync(99L, 42)).Returns(Task.CompletedTask);
            var controller = CreateController(svcMock.Object);

            var result = await controller.MarkAsRead(99L);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task MarkAsRead_Returns401_WhenNoCookie()
        {
            var controller = CreateController(jwtCookie: null);

            var result = await controller.MarkAsRead(1L);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task MarkAsRead_Returns400_WhenServiceThrows()
        {
            var svcMock = new Mock<INotificationService>();
            svcMock.Setup(s => s.MarkAsReadAsync(It.IsAny<long>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("fail"));
            var controller = CreateController(svcMock.Object);

            var result = await controller.MarkAsRead(1L);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // ── MarkAllAsRead ─────────────────────────────────────────────────────

        [Fact]
        public async Task MarkAllAsRead_Returns204_OnSuccess()
        {
            var svcMock = new Mock<INotificationService>();
            svcMock.Setup(s => s.MarkAllAsReadAsync(42)).Returns(Task.CompletedTask);
            var controller = CreateController(svcMock.Object);

            var result = await controller.MarkAllAsRead();

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task MarkAllAsRead_Returns401_WhenNoCookie()
        {
            var controller = CreateController(jwtCookie: null);

            var result = await controller.MarkAllAsRead();

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task MarkAllAsRead_Returns400_WhenServiceThrows()
        {
            var svcMock = new Mock<INotificationService>();
            svcMock.Setup(s => s.MarkAllAsReadAsync(It.IsAny<int>()))
                .ThrowsAsync(new Exception("fail"));
            var controller = CreateController(svcMock.Object);

            var result = await controller.MarkAllAsRead();

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
