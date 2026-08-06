namespace Touir.ExpensesManager.Expenses.Models.Lookups
{
    /// <summary>Recurrence cadence for a recurring expense: Weekly, Monthly, Yearly.</summary>
    public class RecurrenceFrequency : ILookupEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }
}
