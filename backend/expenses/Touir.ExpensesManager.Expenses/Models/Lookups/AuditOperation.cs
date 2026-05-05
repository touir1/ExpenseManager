namespace Touir.ExpensesManager.Expenses.Models.Lookups
{
    /// <summary>Type of audit operation performed on an expense: Add, Update, Delete.</summary>
    public class AuditOperation : ILookupEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }
}
