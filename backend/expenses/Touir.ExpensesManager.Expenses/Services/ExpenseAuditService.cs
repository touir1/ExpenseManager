using Touir.ExpensesManager.Expenses.Infrastructure;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Services
{
    public class ExpenseAuditService : IExpenseAuditService
    {
        // Lookup IDs from seed data (constraints.md)
        private const int OperationAdd = 1;
        private const int OperationUpdate = 2;
        private const int OperationDelete = 3;
        private const int SnapshotBefore = 1;
        private const int SnapshotAfter = 2;

        private readonly ExpensesDbContext _dbContext;

        public ExpenseAuditService(ExpensesDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task WriteAddAuditAsync(Expense expense, int performedById, int performedFromId)
        {
            var log = new ExpenseAuditLog
            {
                ExpenseId = expense.Id,
                OperationId = OperationAdd,
                PerformedAt = DateTime.UtcNow,
                PerformedById = performedById,
                PerformedFromId = performedFromId
            };
            _dbContext.ExpenseAuditLogs.Add(log);
            await _dbContext.SaveChangesAsync();

            _dbContext.ExpenseAuditSnapshots.Add(BuildSnapshot(log.Id, SnapshotAfter, expense));
            await _dbContext.SaveChangesAsync();
        }

        public async Task WriteUpdateAuditAsync(Expense before, Expense after, int performedById, int performedFromId)
        {
            var log = new ExpenseAuditLog
            {
                ExpenseId = after.Id,
                OperationId = OperationUpdate,
                PerformedAt = DateTime.UtcNow,
                PerformedById = performedById,
                PerformedFromId = performedFromId
            };
            _dbContext.ExpenseAuditLogs.Add(log);
            await _dbContext.SaveChangesAsync();

            _dbContext.ExpenseAuditSnapshots.AddRange(
                BuildSnapshot(log.Id, SnapshotBefore, before),
                BuildSnapshot(log.Id, SnapshotAfter, after)
            );
            await _dbContext.SaveChangesAsync();
        }

        public async Task WriteDeleteAuditAsync(Expense expense, int performedById, int performedFromId)
        {
            var log = new ExpenseAuditLog
            {
                ExpenseId = expense.Id,
                OperationId = OperationDelete,
                PerformedAt = DateTime.UtcNow,
                PerformedById = performedById,
                PerformedFromId = performedFromId
            };
            _dbContext.ExpenseAuditLogs.Add(log);
            await _dbContext.SaveChangesAsync();

            _dbContext.ExpenseAuditSnapshots.Add(BuildSnapshot(log.Id, SnapshotBefore, expense));
            await _dbContext.SaveChangesAsync();
        }

        private static ExpenseAuditSnapshot BuildSnapshot(long auditLogId, int snapshotTypeId, Expense expense)
        {
            return new ExpenseAuditSnapshot
            {
                AuditLogId = auditLogId,
                SnapshotTypeId = snapshotTypeId,
                Amount = expense.Amount,
                CurrencyId = expense.CurrencyId,
                Date = expense.Date,
                CategoryId = expense.CategoryId,
                SubcategoryId = expense.SubcategoryId,
                Description = expense.Description
            };
        }
    }
}
