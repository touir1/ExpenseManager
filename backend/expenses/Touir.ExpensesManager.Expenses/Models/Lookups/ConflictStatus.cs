namespace Touir.ExpensesManager.Expenses.Models.Lookups
{
    /// <summary>Resolution state of a currency rate conflict: Pending, Resolved.</summary>
    public class ConflictStatus : ILookupEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }
}
