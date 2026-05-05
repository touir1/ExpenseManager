namespace Touir.ExpensesManager.Expenses.Models.Lookups
{
    /// <summary>Entry point for an expense or audit operation: SingleWeb, SingleMobile, BulkWeb.</summary>
    public class OperationSource : ILookupEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }
}
