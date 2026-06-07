using Touir.ExpensesManager.Notifications.Models;
using Touir.ExpensesManager.Notifications.Repositories;
using Touir.ExpensesManager.Notifications.Tests.TestHelpers;

namespace Touir.ExpensesManager.Notifications.Tests.Repositories
{
    public class InboxRepositoryTests : IDisposable
    {
        private readonly TestNotificationsDbContextWrapper _wrapper;
        private readonly InboxRepository _sut;

        public InboxRepositoryTests()
        {
            _wrapper = new TestNotificationsDbContextWrapper();
            _sut = new InboxRepository(_wrapper.Context);
        }

        public void Dispose()
        {
            _wrapper.Dispose();
            GC.SuppressFinalize(this);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenNotPresent()
        {
            var result = await _sut.ExistsAsync("nonexistent-id");
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsTrue_WhenPresent()
        {
            await _sut.AddAsync(new InboxEvent
            {
                MessageId = "msg-abc",
                EventType = "family.member.removed",
                ReceivedAt = DateTime.UtcNow,
                Status = InboxEventStatus.Processed
            });

            var result = await _sut.ExistsAsync("msg-abc");
            Assert.True(result);
        }

        [Fact]
        public async Task AddAsync_PersistsInboxEvent()
        {
            var ev = new InboxEvent
            {
                MessageId = "msg-xyz",
                EventType = "expenses.import.completed",
                ReceivedAt = DateTime.UtcNow,
                Status = InboxEventStatus.Processed
            };

            await _sut.AddAsync(ev);

            Assert.True(await _sut.ExistsAsync("msg-xyz"));
        }
    }
}
