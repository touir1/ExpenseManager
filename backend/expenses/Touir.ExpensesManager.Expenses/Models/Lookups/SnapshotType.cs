namespace Touir.ExpensesManager.Expenses.Models.Lookups
{
    /// <summary>Snapshot position in an audit event: Before, After.</summary>
    public class SnapshotType : ILookupEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }
}
