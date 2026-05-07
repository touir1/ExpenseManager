using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Repositories;
using Touir.ExpensesManager.Expenses.Tests.TestHelpers;

namespace Touir.ExpensesManager.Expenses.Tests.Repositories
{
    public class InboxRepositoryTests
    {
        #region ExistsAsync Tests

        [Fact]
        public async Task ExistsAsync_ReturnsTrue_WhenMessageExists()
        {
            using var db = new TestExpensesDbContextWrapper();
            db.Context.InboxEvents.Add(new InboxEvent
            {
                MessageId = "msg-001",
                EventType = "user.created",
                ReceivedAt = DateTime.UtcNow,
                Status = InboxEventStatus.Processed
            });
            db.Context.SaveChanges();

            var repo = new InboxRepository(db.Context);
            var exists = await repo.ExistsAsync("msg-001");

            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenMessageDoesNotExist()
        {
            using var db = new TestExpensesDbContextWrapper();
            var repo = new InboxRepository(db.Context);

            var exists = await repo.ExistsAsync("nonexistent-msg");

            Assert.False(exists);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenDifferentMessageIdExists()
        {
            using var db = new TestExpensesDbContextWrapper();
            db.Context.InboxEvents.Add(new InboxEvent
            {
                MessageId = "msg-abc",
                EventType = "user.created",
                ReceivedAt = DateTime.UtcNow,
                Status = InboxEventStatus.Processed
            });
            db.Context.SaveChanges();

            var repo = new InboxRepository(db.Context);
            var exists = await repo.ExistsAsync("msg-xyz");

            Assert.False(exists);
        }

        #endregion

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_SavesEventToDatabase()
        {
            using var db = new TestExpensesDbContextWrapper();
            var repo = new InboxRepository(db.Context);

            await repo.AddAsync(new InboxEvent
            {
                MessageId = "msg-save",
                EventType = "user.created",
                ReceivedAt = DateTime.UtcNow,
                Status = InboxEventStatus.Processed
            });

            Assert.Single(db.Context.InboxEvents);
        }

        [Fact]
        public async Task AddAsync_SavesCorrectFields()
        {
            using var db = new TestExpensesDbContextWrapper();
            var repo = new InboxRepository(db.Context);
            var receivedAt = DateTime.UtcNow;

            await repo.AddAsync(new InboxEvent
            {
                MessageId = "msg-fields",
                EventType = "user.deleted",
                ReceivedAt = receivedAt,
                Status = InboxEventStatus.Processed,
                Error = null
            });

            var saved = db.Context.InboxEvents.Single();
            Assert.Equal("msg-fields", saved.MessageId);
            Assert.Equal("user.deleted", saved.EventType);
            Assert.Equal(InboxEventStatus.Processed, saved.Status);
            Assert.Null(saved.Error);
        }

        [Fact]
        public async Task AddAsync_SavesError_WhenProvided()
        {
            using var db = new TestExpensesDbContextWrapper();
            var repo = new InboxRepository(db.Context);

            await repo.AddAsync(new InboxEvent
            {
                MessageId = "msg-err",
                EventType = "user.created",
                ReceivedAt = DateTime.UtcNow,
                Status = InboxEventStatus.Failed,
                Error = "deserialization failed"
            });

            var saved = db.Context.InboxEvents.Single();
            Assert.Equal(InboxEventStatus.Failed, saved.Status);
            Assert.Equal("deserialization failed", saved.Error);
        }

        [Fact]
        public async Task AddAsync_MakesEventVisibleToExistsAsync()
        {
            using var db = new TestExpensesDbContextWrapper();
            var repo = new InboxRepository(db.Context);

            await repo.AddAsync(new InboxEvent
            {
                MessageId = "msg-roundtrip",
                EventType = "user.updated",
                ReceivedAt = DateTime.UtcNow,
                Status = InboxEventStatus.Processed
            });

            var exists = await repo.ExistsAsync("msg-roundtrip");
            Assert.True(exists);
        }

        #endregion
    }
}
