namespace Touir.ExpensesManager.Expenses.Models.Lookups
{
    /// <summary>Member role within a family: Head, Member.</summary>
    public class FamilyRole : ILookupEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }
}
