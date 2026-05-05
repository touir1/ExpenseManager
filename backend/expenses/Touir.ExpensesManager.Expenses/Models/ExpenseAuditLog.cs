using Touir.ExpensesManager.Expenses.Models.Lookups;

namespace Touir.ExpensesManager.Expenses.Models
{
    public class ExpenseAuditLog
    {
        public long Id { get; set; }
        public long ExpenseId { get; set; }
        public int OperationId { get; set; }
        public DateTime PerformedAt { get; set; }
        public int PerformedById { get; set; }
        public int PerformedFromId { get; set; }

        public Expense Expense { get; set; } = null!;
        public AuditOperation Operation { get; set; } = null!;
        public OperationSource PerformedFrom { get; set; } = null!;
    }
}
