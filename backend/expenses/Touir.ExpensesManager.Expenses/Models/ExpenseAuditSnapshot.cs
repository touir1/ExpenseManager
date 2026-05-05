using Touir.ExpensesManager.Expenses.Models.Lookups;

namespace Touir.ExpensesManager.Expenses.Models
{
    public class ExpenseAuditSnapshot
    {
        public long Id { get; set; }
        public long AuditLogId { get; set; }
        public int SnapshotTypeId { get; set; }
        public decimal Amount { get; set; }
        public int CurrencyId { get; set; }
        public DateOnly Date { get; set; }
        public int? CategoryId { get; set; }
        public int? SubcategoryId { get; set; }
        public string? Description { get; set; }
        public string? Tags { get; set; }
        public string? Families { get; set; }

        public ExpenseAuditLog AuditLog { get; set; } = null!;
        public Currency Currency { get; set; } = null!;
        public SnapshotType SnapshotType { get; set; } = null!;
    }
}
