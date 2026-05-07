using Microsoft.AspNetCore.Mvc;
using Moq;
using Touir.ExpensesManager.Users.Controllers;
using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Repositories.Contracts;

namespace Touir.ExpensesManager.Users.Tests.Controllers
{
    public class MessagingControllerTests
    {
        private static MessagingController CreateController(IOutboxRepository? outboxRepo = null) =>
            new(outboxRepo ?? Mock.Of<IOutboxRepository>());

        #region Replay Tests

        [Fact]
        public async Task Replay_ReturnsOk_WithRequeuedCount()
        {
            var outboxRepo = new Mock<IOutboxRepository>();
            outboxRepo.Setup(r => r.RequeueAsync(null, null, false)).ReturnsAsync(3);

            var controller = CreateController(outboxRepo.Object);
            var result = await controller.Replay(null, null, false);

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = ok.Value!;
            var requeued = (int)value.GetType().GetProperty("requeued")!.GetValue(value)!;
            Assert.Equal(3, requeued);
        }

        [Fact]
        public async Task Replay_PassesEventTypeFilter_ToRepository()
        {
            var outboxRepo = new Mock<IOutboxRepository>();
            outboxRepo.Setup(r => r.RequeueAsync("user.created", null, false)).ReturnsAsync(1);

            var controller = CreateController(outboxRepo.Object);
            await controller.Replay("user.created", null, false);

            outboxRepo.Verify(r => r.RequeueAsync("user.created", null, false), Times.Once);
        }

        [Fact]
        public async Task Replay_PassesForceAll_ToRepository()
        {
            var outboxRepo = new Mock<IOutboxRepository>();
            outboxRepo.Setup(r => r.RequeueAsync(null, null, true)).ReturnsAsync(5);

            var controller = CreateController(outboxRepo.Object);
            await controller.Replay(null, null, forceAll: true);

            outboxRepo.Verify(r => r.RequeueAsync(null, null, true), Times.Once);
        }

        [Fact]
        public async Task Replay_ReturnsZero_WhenNothingRequeued()
        {
            var outboxRepo = new Mock<IOutboxRepository>();
            outboxRepo.Setup(r => r.RequeueAsync(null, null, false)).ReturnsAsync(0);

            var controller = CreateController(outboxRepo.Object);
            var result = await controller.Replay(null, null, false);

            var ok = Assert.IsType<OkObjectResult>(result);
            var requeued = (int)ok.Value!.GetType().GetProperty("requeued")!.GetValue(ok.Value)!;
            Assert.Equal(0, requeued);
        }

        #endregion

        #region Stats Tests

        [Fact]
        public async Task Stats_ReturnsOk_WithPendingCount()
        {
            var events = new List<OutboxEvent>
            {
                new() { MessageId = "m1", EventType = "e", Payload = "{}", CreatedAt = DateTime.UtcNow },
                new() { MessageId = "m2", EventType = "e", Payload = "{}", CreatedAt = DateTime.UtcNow }
            };
            var outboxRepo = new Mock<IOutboxRepository>();
            outboxRepo.Setup(r => r.GetPendingAsync(5)).ReturnsAsync(events);

            var controller = CreateController(outboxRepo.Object);
            var result = await controller.Stats();

            var ok = Assert.IsType<OkObjectResult>(result);
            var pending = (int)ok.Value!.GetType().GetProperty("pending")!.GetValue(ok.Value)!;
            Assert.Equal(2, pending);
        }

        [Fact]
        public async Task Stats_ReturnsZeroPending_WhenNoEvents()
        {
            var outboxRepo = new Mock<IOutboxRepository>();
            outboxRepo.Setup(r => r.GetPendingAsync(5)).ReturnsAsync(new List<OutboxEvent>());

            var controller = CreateController(outboxRepo.Object);
            var result = await controller.Stats();

            var ok = Assert.IsType<OkObjectResult>(result);
            var pending = (int)ok.Value!.GetType().GetProperty("pending")!.GetValue(ok.Value)!;
            Assert.Equal(0, pending);
        }

        #endregion
    }
}
