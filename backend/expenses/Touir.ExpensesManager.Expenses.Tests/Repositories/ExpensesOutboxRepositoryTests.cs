using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Repositories;
using Touir.ExpensesManager.Expenses.Tests.TestHelpers;

// Uses EnsureCreated (not Migrate) because OutboxEvent.Id is long and the Npgsql migration
// uses IDENTITY ALWAYS which is not compatible with SQLite's rowid aliasing via table constraints.

namespace Touir.ExpensesManager.Expenses.Tests.Repositories
{
    public class ExpensesOutboxRepositoryTests
    {
        private static OutboxEvent MakeEvent(string msgId = "msg-1", string eventType = "test.event") => new()
        {
            MessageId = msgId,
            EventType = eventType,
            Payload = "{}",
            CreatedAt = DateTime.UtcNow
        };

        // ── EnqueueAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task EnqueueAsync_PersistsEvent()
        {
            using var db = new TestExpensesDbContextEnsureCreated();
            var sut = new ExpensesOutboxRepository(db.Context);
            var evt = MakeEvent("enqueue-1");

            await sut.EnqueueAsync(evt);

            Assert.True(db.Context.OutboxEvents.Any(e => e.MessageId == "enqueue-1"));
        }

        // ── GetPendingAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task GetPendingAsync_ReturnsUnpublishedEvents()
        {
            using var db = new TestExpensesDbContextEnsureCreated();
            var sut = new ExpensesOutboxRepository(db.Context);
            db.Context.OutboxEvents.Add(MakeEvent("pending-1"));
            db.Context.OutboxEvents.Add(new OutboxEvent
            {
                MessageId = "published-1",
                EventType = "test",
                Payload = "{}",
                CreatedAt = DateTime.UtcNow,
                PublishedAt = DateTime.UtcNow
            });
            await db.Context.SaveChangesAsync();

            var result = (await sut.GetPendingAsync(maxRetries: 5)).ToList();

            Assert.Contains(result, e => e.MessageId == "pending-1");
            Assert.DoesNotContain(result, e => e.MessageId == "published-1");
        }

        [Fact]
        public async Task GetPendingAsync_ExceedsMaxRetries_NotReturned()
        {
            using var db = new TestExpensesDbContextEnsureCreated();
            var sut = new ExpensesOutboxRepository(db.Context);
            db.Context.OutboxEvents.Add(new OutboxEvent
            {
                MessageId = "exhausted-1",
                EventType = "test",
                Payload = "{}",
                CreatedAt = DateTime.UtcNow,
                RetryCount = 5
            });
            await db.Context.SaveChangesAsync();

            var result = (await sut.GetPendingAsync(maxRetries: 5)).ToList();

            Assert.DoesNotContain(result, e => e.MessageId == "exhausted-1");
        }

        // ── MarkPublishedAsync ────────────────────────────────────────────────

        [Fact]
        public async Task MarkPublishedAsync_SetsPublishedAt_WhenFound()
        {
            using var db = new TestExpensesDbContextEnsureCreated();
            var sut = new ExpensesOutboxRepository(db.Context);
            var evt = MakeEvent("mark-pub-1");
            db.Context.OutboxEvents.Add(evt);
            await db.Context.SaveChangesAsync();

            await sut.MarkPublishedAsync(evt.Id);

            var updated = db.Context.OutboxEvents.Find(evt.Id);
            Assert.NotNull(updated!.PublishedAt);
        }

        [Fact]
        public async Task MarkPublishedAsync_DoesNotThrow_WhenNotFound()
        {
            using var db = new TestExpensesDbContextEnsureCreated();
            var sut = new ExpensesOutboxRepository(db.Context);

            var ex = await Record.ExceptionAsync(() => sut.MarkPublishedAsync(99999L));

            Assert.Null(ex);
        }

        // ── MarkFailedAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task MarkFailedAsync_IncrementsRetryCountAndSetsError_WhenFound()
        {
            using var db = new TestExpensesDbContextEnsureCreated();
            var sut = new ExpensesOutboxRepository(db.Context);
            var evt = MakeEvent("mark-fail-1");
            db.Context.OutboxEvents.Add(evt);
            await db.Context.SaveChangesAsync();

            await sut.MarkFailedAsync(evt.Id, "connection refused");

            var updated = db.Context.OutboxEvents.Find(evt.Id);
            Assert.Equal(1, updated!.RetryCount);
            Assert.Equal("connection refused", updated.LastError);
        }

        [Fact]
        public async Task MarkFailedAsync_TruncatesLongError()
        {
            using var db = new TestExpensesDbContextEnsureCreated();
            var sut = new ExpensesOutboxRepository(db.Context);
            var evt = MakeEvent("mark-fail-2");
            db.Context.OutboxEvents.Add(evt);
            await db.Context.SaveChangesAsync();
            var longError = new string('X', 3000);

            await sut.MarkFailedAsync(evt.Id, longError);

            var updated = db.Context.OutboxEvents.Find(evt.Id);
            Assert.Equal(2000, updated!.LastError!.Length);
        }

        [Fact]
        public async Task MarkFailedAsync_DoesNotThrow_WhenNotFound()
        {
            using var db = new TestExpensesDbContextEnsureCreated();
            var sut = new ExpensesOutboxRepository(db.Context);

            var ex = await Record.ExceptionAsync(() => sut.MarkFailedAsync(99999L, "err"));

            Assert.Null(ex);
        }
    }
}
