using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Touir.ExpensesManager.Expenses.Infrastructure;

namespace Touir.ExpensesManager.Expenses.Tests.TestHelpers
{
    public class TestExpensesDbContextEnsureCreated : IDisposable
    {
        public ExpensesDbContext Context { get; }
        private readonly SqliteConnection _connection;

        public TestExpensesDbContextEnsureCreated()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            var options = new DbContextOptionsBuilder<ExpensesDbContext>()
                .UseSqlite(_connection)
                .Options;
            Context = new ExpensesDbContext(options);
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
