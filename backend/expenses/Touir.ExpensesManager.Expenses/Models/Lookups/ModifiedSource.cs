namespace Touir.ExpensesManager.Expenses.Models.Lookups
{
    /// <summary>Channel through which an expense was modified: Web, Mobile.</summary>
    public class ModifiedSource : ILookupEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }
}
