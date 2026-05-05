namespace Touir.ExpensesManager.Expenses.Models.Lookups
{
    /// <summary>How a currency rate conflict was resolved: AcceptAuto, KeepManual, Custom.</summary>
    public class ConflictResolution : ILookupEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }
}
