namespace Touir.ExpensesManager.Expenses.Models
{
    public class ExpenseFamilyAttribution
    {
        public long Id { get; set; }
        public long ExpenseId { get; set; }
        public int FamilyId { get; set; }
        public DateTime AttributedAt { get; set; }
        public int AttributedById { get; set; }

        public Expense Expense { get; set; } = null!;
        public Family Family { get; set; } = null!;
    }
}
