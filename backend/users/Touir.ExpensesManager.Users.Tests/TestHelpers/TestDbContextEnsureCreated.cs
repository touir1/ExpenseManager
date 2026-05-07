using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Touir.ExpensesManager.Users.Infrastructure;

namespace Touir.ExpensesManager.Users.Tests.Repositories.Helpers
{
    /// <summary>
    /// Uses EnsureCreated instead of Migrate so SQLite correctly maps long PKs
    /// (bigint in Npgsql migrations does not auto-generate in SQLite).
    /// </summary>
    public class TestDbContextEnsureCreated : IDisposable
    {
        public UsersAppDbContext Context { get; }
        private readonly SqliteConnection _connection;

        public TestDbContextEnsureCreated()
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
            GC.SuppressFinalize(this);
        }
    }
}
