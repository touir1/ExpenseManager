using Touir.ExpensesManager.Notifications.Models;
using Touir.ExpensesManager.Notifications.Repositories;
using Touir.ExpensesManager.Notifications.Tests.TestHelpers;

namespace Touir.ExpensesManager.Notifications.Tests.Repositories
{
    public class NotificationRepositoryTests : IDisposable
    {
        private readonly TestNotificationsDbContextWrapper _wrapper;
        private readonly NotificationRepository _sut;

        public NotificationRepositoryTests()
        {
            _wrapper = new TestNotificationsDbContextWrapper();
            _sut = new NotificationRepository(_wrapper.Context);
        }

        public void Dispose()
        {
            _wrapper.Dispose();
            GC.SuppressFinalize(this);
        }

        private static Notification Make(int userId = 1) => new()
        {
            UserId = userId,
            Type = NotificationType.FamilyMemberRemoved,
            Payload = """{"type":"FAMILY_MEMBER_REMOVED","familyId":1,"familyName":"F","removedByUserId":2,"removedByName":"A","expenseCount":3}""",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        [Fact]
        public async Task CreateAsync_PersistsAndReturnsWithId()
        {
            var n = await _sut.CreateAsync(Make());

            Assert.True(n.Id > 0);
            Assert.Equal(NotificationType.FamilyMemberRemoved, n.Type);
        }

        [Fact]
        public async Task GetByUserAsync_ReturnsOnlyForUser_NewestFirst()
        {
            var n1 = Make(userId: 1); n1.CreatedAt = DateTime.UtcNow.AddMinutes(-2);
            var n2 = Make(userId: 1); n2.CreatedAt = DateTime.UtcNow.AddMinutes(-1);
            var other = Make(userId: 99);
            await _sut.CreateAsync(n1);
            await _sut.CreateAsync(n2);
            await _sut.CreateAsync(other);

            var results = (await _sut.GetByUserAsync(userId: 1, page: 1, pageSize: 10)).ToList();

            Assert.Equal(2, results.Count);
            Assert.True(results[0].CreatedAt >= results[1].CreatedAt);
        }

        [Fact]
        public async Task GetUnreadCountAsync_CountsOnlyUnread()
        {
            var unread1 = Make(userId: 1);
            var unread2 = Make(userId: 1);
            var read = Make(userId: 1); read.IsRead = true;
            await _sut.CreateAsync(unread1);
            await _sut.CreateAsync(unread2);
            await _sut.CreateAsync(read);

            var count = await _sut.GetUnreadCountAsync(userId: 1);

            Assert.Equal(2, count);
        }

        [Fact]
        public async Task MarkAsReadAsync_SetsIsReadAndReadAt()
        {
            var n = await _sut.CreateAsync(Make(userId: 1));

            await _sut.MarkAsReadAsync(n.Id, userId: 1);

            var result = await _sut.GetByIdAsync(n.Id, userId: 1);
            Assert.NotNull(result);
            Assert.True(result!.IsRead);
            Assert.NotNull(result.ReadAt);
        }

        [Fact]
        public async Task MarkAsReadAsync_WrongUser_DoesNothing()
        {
            var n = await _sut.CreateAsync(Make(userId: 1));

            await _sut.MarkAsReadAsync(n.Id, userId: 999);

            var result = await _sut.GetByIdAsync(n.Id, userId: 1);
            Assert.NotNull(result);
            Assert.False(result!.IsRead);
        }

        [Fact]
        public async Task MarkAllAsReadAsync_MarksAllForUser()
        {
            await _sut.CreateAsync(Make(userId: 1));
            await _sut.CreateAsync(Make(userId: 1));
            await _sut.CreateAsync(Make(userId: 99));

            await _sut.MarkAllAsReadAsync(userId: 1);

            Assert.Equal(0, await _sut.GetUnreadCountAsync(userId: 1));
            Assert.Equal(1, await _sut.GetUnreadCountAsync(userId: 99));
        }

        [Fact]
        public async Task GetByUserAsync_Pagination_Works()
        {
            for (var i = 0; i < 5; i++)
                await _sut.CreateAsync(Make(userId: 1));

            var page1 = (await _sut.GetByUserAsync(userId: 1, page: 1, pageSize: 3)).ToList();
            var page2 = (await _sut.GetByUserAsync(userId: 1, page: 2, pageSize: 3)).ToList();

            Assert.Equal(3, page1.Count);
            Assert.Equal(2, page2.Count);
        }
    }
}
