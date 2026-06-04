using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Touir.ExpensesManager.Notifications.Infrastructure;

namespace Touir.ExpensesManager.Notifications.Tests.TestHelpers
{
    public class TestNotificationsDbContextWrapper : IDisposable
    {
        public NotificationsDbContext Context { get; }
        private readonly SqliteConnection _connection;

        public TestNotificationsDbContextWrapper()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            var options = new DbContextOptionsBuilder<NotificationsDbContext>()
                .UseSqlite(_connection)
                .Options;
            Context = new NotificationsDbContext(options);
            Context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            Context.Dispose();
            _connection.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
