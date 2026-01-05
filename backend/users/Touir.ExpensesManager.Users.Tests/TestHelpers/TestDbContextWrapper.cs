using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Touir.ExpensesManager.Users.Infrastructure;

namespace Touir.ExpensesManager.Users.Tests.Repositories.Helpers
{
    public class TestDbContextWrapper : IDisposable
    {
        public UsersAppDbContext Context { get; }
        private readonly SqliteConnection _connection;

        public TestDbContextWrapper()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            var options = new DbContextOptionsBuilder<UsersAppDbContext>()
                .UseSqlite(_connection)
                .Options;
            Context = new UsersAppDbContext(options);
            Context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            Context.Dispose();
            _connection.Dispose();
        }
    }
}
