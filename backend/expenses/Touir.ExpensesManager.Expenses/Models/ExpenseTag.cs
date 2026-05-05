namespace Touir.ExpensesManager.Expenses.Models
{
    public class ExpenseTag
    {
        public long ExpenseId { get; set; }
        public int TagId { get; set; }

        public Expense Expense { get; set; } = null!;
        public Tag Tag { get; set; } = null!;
    }
}
