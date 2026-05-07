using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Repositories;
using Touir.ExpensesManager.Users.Tests.Repositories.Helpers; // TestDbContextEnsureCreated

namespace Touir.ExpensesManager.Users.Tests.Repositories
{
    public class OutboxRepositoryTests
    {
        private static OutboxEvent MakeEvent(string messageId = "msg-1", string eventType = "user.created", DateTime? createdAt = null) =>
            new()
            {
                MessageId = messageId,
                EventType = eventType,
                Payload = "{}",
                CreatedAt = createdAt ?? DateTime.UtcNow,
                RetryCount = 0
            };

        #region EnqueueAsync Tests

        [Fact]
        public async Task EnqueueAsync_SavesEventToDatabase()
        {
            using var db = new TestDbContextEnsureCreated();
            var repo = new OutboxRepository(db.Context);

            await repo.EnqueueAsync(MakeEvent());

            Assert.Single(db.Context.OutboxEvents);
        }

        [Fact]
        public async Task EnqueueAsync_SavesCorrectFields()
        {
            using var db = new TestDbContextEnsureCreated();
            var repo = new OutboxRepository(db.Context);
            var created = DateTime.UtcNow;

            await repo.EnqueueAsync(new OutboxEvent
            {
                MessageId = "abc-123",
                EventType = "user.created",
                Payload = "{\"userId\":42}",
                CreatedAt = created,
                RetryCount = 0
            });

            var saved = db.Context.OutboxEvents.Single();
            Assert.Equal("abc-123", saved.MessageId);
            Assert.Equal("user.created", saved.EventType);
            Assert.Equal("{\"userId\":42}", saved.Payload);
            Assert.Null(saved.PublishedAt);
            Assert.Equal(0, saved.RetryCount);
        }

        #endregion

        #region GetPendingAsync Tests

        [Fact]
        public async Task GetPendingAsync_ReturnsOnlyUnpublishedEventsUnderMaxRetries()
        {
            using var db = new TestDbContextEnsureCreated();
            var repo = new OutboxRepository(db.Context);

            db.Context.OutboxEvents.AddRange(
                new OutboxEvent { MessageId = "m1", EventType = "e", Payload = "{}", CreatedAt = DateTime.UtcNow, RetryCount = 0 },
                new OutboxEvent { MessageId = "m2", EventType = "e", Payload = "{}", CreatedAt = DateTime.UtcNow, RetryCount = 3, PublishedAt = null },
                new OutboxEvent { MessageId = "m3", EventType = "e", Payload = "{}", CreatedAt = DateTime.UtcNow, RetryCount = 0, PublishedAt = DateTime.UtcNow }
            );
            db.Context.SaveChanges();

            var pending = await repo.GetPendingAsync(maxRetries: 5);

            Assert.Equal(2, pending.Count);
            Assert.DoesNotContain(pending, e => e.MessageId == "m3");
        }

        [Fact]
        public async Task GetPendingAsync_ExcludesEventsAtOrAboveMaxRetries()
        {
            using var db = new TestDbContextEnsureCreated();
            var repo = new OutboxRepository(db.Context);

            db.Context.OutboxEvents.AddRange(
                new OutboxEvent { MessageId = "ok", EventType = "e", Payload = "{}", CreatedAt = DateTime.UtcNow, RetryCount = 4 },
                new OutboxEvent { MessageId = "over", EventType = "e", Payload = "{}", CreatedAt = DateTime.UtcNow, RetryCount = 5 }
            );
            db.Context.SaveChanges();

            var pending = await repo.GetPendingAsync(maxRetries: 5);

            Assert.Single(pending);
            Assert.Equal("ok", pending[0].MessageId);
        }

        [Fact]
        public async Task GetPendingAsync_ReturnsEmpty_WhenNoPendingEvents()
        {
            using var db = new TestDbContextEnsureCreated();
            var repo = new OutboxRepository(db.Context);

            var pending = await repo.GetPendingAsync(maxRetries: 5);

            Assert.Empty(pending);
        }

        #endregion

        #region MarkPublishedAsync Tests

        [Fact]
        public async Task MarkPublishedAsync_SetsPublishedAt()
        {
            using var db = new TestDbContextEnsureCreated();
            db.Context.OutboxEvents.Add(MakeEvent());
            db.Context.SaveChanges();
            var id = db.Context.OutboxEvents.Single().Id;

            var repo = new OutboxRepository(db.Context);
            await repo.MarkPublishedAsync(id);

            var evt = db.Context.OutboxEvents.Find(id);
            Assert.NotNull(evt!.PublishedAt);
        }

        [Fact]
        public async Task MarkPublishedAsync_DoesNothing_WhenEventNotFound()
        {
            using var db = new TestDbContextEnsureCreated();
            var repo = new OutboxRepository(db.Context);

            await repo.MarkPublishedAsync(999);

            Assert.Empty(db.Context.OutboxEvents);
        }

        #endregion

        #region MarkFailedAsync Tests

        [Fact]
        public async Task MarkFailedAsync_IncrementsRetryCount()
        {
            using var db = new TestDbContextEnsureCreated();
            db.Context.OutboxEvents.Add(MakeEvent());
            db.Context.SaveChanges();
            var id = db.Context.OutboxEvents.Single().Id;

            var repo = new OutboxRepository(db.Context);
            await repo.MarkFailedAsync(id, "timeout");

            var evt = db.Context.OutboxEvents.Find(id);
            Assert.Equal(1, evt!.RetryCount);
        }

        [Fact]
        public async Task MarkFailedAsync_StoresError()
        {
            using var db = new TestDbContextEnsureCreated();
            db.Context.OutboxEvents.Add(MakeEvent());
            db.Context.SaveChanges();
            var id = db.Context.OutboxEvents.Single().Id;

            var repo = new OutboxRepository(db.Context);
            await repo.MarkFailedAsync(id, "connection refused");

            var evt = db.Context.OutboxEvents.Find(id);
            Assert.Equal("connection refused", evt!.LastError);
        }

        [Fact]
        public async Task MarkFailedAsync_TruncatesErrorAt2000Chars()
        {
            using var db = new TestDbContextEnsureCreated();
            db.Context.OutboxEvents.Add(MakeEvent());
            db.Context.SaveChanges();
            var id = db.Context.OutboxEvents.Single().Id;
            var longError = new string('x', 3000);

            var repo = new OutboxRepository(db.Context);
            await repo.MarkFailedAsync(id, longError);

            var evt = db.Context.OutboxEvents.Find(id);
            Assert.Equal(2000, evt!.LastError!.Length);
        }

        [Fact]
        public async Task MarkFailedAsync_DoesNothing_WhenEventNotFound()
        {
            using var db = new TestDbContextEnsureCreated();
            var repo = new OutboxRepository(db.Context);

            await repo.MarkFailedAsync(999, "error");

            Assert.Empty(db.Context.OutboxEvents);
        }

        #endregion

        #region RequeueAsync Tests

        [Fact]
        public async Task RequeueAsync_ResetsPublishedAtAndRetryCount()
        {
            using var db = new TestDbContextEnsureCreated();
            db.Context.OutboxEvents.Add(new OutboxEvent
            {
                MessageId = "m1", EventType = "user.created", Payload = "{}", CreatedAt = DateTime.UtcNow,
                PublishedAt = DateTime.UtcNow, RetryCount = 3, LastError = "err"
            });
            db.Context.SaveChanges();

            var repo = new OutboxRepository(db.Context);
            var count = await repo.RequeueAsync(null, null, forceAll: true);

            Assert.Equal(1, count);
            var evt = db.Context.OutboxEvents.Single();
            Assert.Null(evt.PublishedAt);
            Assert.Equal(0, evt.RetryCount);
            Assert.Null(evt.LastError);
        }

        [Fact]
        public async Task RequeueAsync_FiltersBy_EventType()
        {
            using var db = new TestDbContextEnsureCreated();
            db.Context.OutboxEvents.AddRange(
                new OutboxEvent { MessageId = "m1", EventType = "user.created", Payload = "{}", CreatedAt = DateTime.UtcNow, RetryCount = 3, LastError = "e" },
                new OutboxEvent { MessageId = "m2", EventType = "user.deleted", Payload = "{}", CreatedAt = DateTime.UtcNow, RetryCount = 3, LastError = "e" }
            );
            db.Context.SaveChanges();

            var repo = new OutboxRepository(db.Context);
            var count = await repo.RequeueAsync("user.created", null, forceAll: false);

            Assert.Equal(1, count);
        }

        [Fact]
        public async Task RequeueAsync_FiltersBy_FromDate()
        {
            using var db = new TestDbContextEnsureCreated();
            var cutoff = DateTime.UtcNow;
            db.Context.OutboxEvents.AddRange(
                new OutboxEvent { MessageId = "old", EventType = "e", Payload = "{}", CreatedAt = cutoff.AddHours(-2), RetryCount = 2, LastError = "e" },
                new OutboxEvent { MessageId = "new", EventType = "e", Payload = "{}", CreatedAt = cutoff.AddHours(1), RetryCount = 2, LastError = "e" }
            );
            db.Context.SaveChanges();

            var repo = new OutboxRepository(db.Context);
            var count = await repo.RequeueAsync(null, cutoff, forceAll: false);

            Assert.Equal(1, count);
            Assert.Equal(0, db.Context.OutboxEvents.Single(e => e.MessageId == "new").RetryCount);
        }

        [Fact]
        public async Task RequeueAsync_WithForceAllFalse_SkipsSuccessfullyPublishedWithNoError()
        {
            using var db = new TestDbContextEnsureCreated();
            db.Context.OutboxEvents.Add(new OutboxEvent
            {
                MessageId = "done", EventType = "e", Payload = "{}", CreatedAt = DateTime.UtcNow,
                PublishedAt = DateTime.UtcNow, RetryCount = 0, LastError = null
            });
            db.Context.SaveChanges();

            var repo = new OutboxRepository(db.Context);
            var count = await repo.RequeueAsync(null, null, forceAll: false);

            Assert.Equal(0, count);
        }

        #endregion
    }
}
