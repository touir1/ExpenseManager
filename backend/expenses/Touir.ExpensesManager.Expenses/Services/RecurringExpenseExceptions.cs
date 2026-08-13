namespace Touir.ExpensesManager.Expenses.Services
{
    public class RecurringExpenseNotDueException : Exception
    {
        public RecurringExpenseNotDueException(string message = ServiceErrors.RecurringExpenseNotDue) : base(message) { }
    }
}
