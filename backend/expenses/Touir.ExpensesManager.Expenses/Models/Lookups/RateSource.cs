namespace Touir.ExpensesManager.Expenses.Models.Lookups
{
    /// <summary>Origin of a currency rate entry: Auto, Manual.</summary>
    public class RateSource : ILookupEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }
}
