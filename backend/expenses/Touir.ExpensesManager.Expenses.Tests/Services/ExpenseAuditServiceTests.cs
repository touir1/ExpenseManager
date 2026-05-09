using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Services;
using Touir.ExpensesManager.Expenses.Tests.TestHelpers;

namespace Touir.ExpensesManager.Expenses.Tests.Services
{
    public class ExpenseAuditServiceTests : IDisposable
    {
        private readonly TestExpensesDbContextWrapper _wrapper;
        private readonly ExpenseAuditService _sut;

        public ExpenseAuditServiceTests()
        {
            _wrapper = new TestExpensesDbContextWrapper();
            _sut = new ExpenseAuditService(_wrapper.Context);
        }

        public void Dispose()
        {
            _wrapper.Dispose();
            GC.SuppressFinalize(this);
        }

        private async Task<Expense> SeedExpenseAsync()
        {
            var user = new Models.External.User { Id = 1, FirstName = "T", LastName = "U", Email = "t@u.com", IsDeleted = false };
            _wrapper.Context.Users.Add(user);
            var currency = new Currency { Id = 1000, Code = "TST", Name = "Test", Symbol = "$", Decimals = 2 };
            _wrapper.Context.Currencies.Add(currency);
            await _wrapper.Context.SaveChangesAsync();

            var expense = new Expense
            {
                Id = 1,
                UserId = 1,
                Amount = 50m,
                CurrencyId = 1000,
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                CreatedAt = DateTime.UtcNow,
                CreatedById = 1,
                CreatedFromId = 1
            };
            _wrapper.Context.Expenses.Add(expense);
            await _wrapper.Context.SaveChangesAsync();
            return expense;
        }

        [Fact]
        public async Task WriteAddAuditAsync_CreatesLogAndAfterSnapshot()
        {
            var expense = await SeedExpenseAsync();

            await _sut.WriteAddAuditAsync(expense, performedById: 1, performedFromId: 1);

            var log = _wrapper.Context.ExpenseAuditLogs.Single();
            Assert.Equal(1, log.OperationId); // Add
            Assert.Equal(expense.Id, log.ExpenseId);

            var snapshot = _wrapper.Context.ExpenseAuditSnapshots.Single();
            Assert.Equal(2, snapshot.SnapshotTypeId); // After
            Assert.Equal(expense.Amount, snapshot.Amount);
        }

        [Fact]
        public async Task WriteUpdateAuditAsync_CreatesLogAndTwoSnapshots()
        {
            var expense = await SeedExpenseAsync();
            var before = new Expense { Id = expense.Id, UserId = expense.UserId, Amount = 30m, CurrencyId = expense.CurrencyId, Date = expense.Date, CreatedAt = expense.CreatedAt, CreatedById = expense.CreatedById, CreatedFromId = expense.CreatedFromId };
            var after = expense;
            after.Amount = 80m;

            await _sut.WriteUpdateAuditAsync(before, after, performedById: 1, performedFromId: 1);

            var log = _wrapper.Context.ExpenseAuditLogs.Single();
            Assert.Equal(2, log.OperationId); // Update

            var snapshots = _wrapper.Context.ExpenseAuditSnapshots.ToList();
            Assert.Equal(2, snapshots.Count);
            Assert.Contains(snapshots, s => s.SnapshotTypeId == 1 && s.Amount == 30m); // Before
            Assert.Contains(snapshots, s => s.SnapshotTypeId == 2 && s.Amount == 80m); // After
        }

        [Fact]
        public async Task WriteDeleteAuditAsync_CreatesLogAndBeforeSnapshot()
        {
            var expense = await SeedExpenseAsync();

            await _sut.WriteDeleteAuditAsync(expense, performedById: 1, performedFromId: 1);

            var log = _wrapper.Context.ExpenseAuditLogs.Single();
            Assert.Equal(3, log.OperationId); // Delete

            var snapshot = _wrapper.Context.ExpenseAuditSnapshots.Single();
            Assert.Equal(1, snapshot.SnapshotTypeId); // Before
        }
    }
}
