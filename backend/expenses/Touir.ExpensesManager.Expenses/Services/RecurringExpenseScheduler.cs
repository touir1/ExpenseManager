namespace Touir.ExpensesManager.Expenses.Services
{
    /// <summary>
    /// Shared scheduling logic for recurring expense templates.
    /// RecurrenceFrequency seed: 1=Weekly, 2=Monthly, 3=Yearly.
    /// </summary>
    public static class RecurringExpenseScheduler
    {
        private const int Weekly = 1;
        private const int Monthly = 2;
        private const int Yearly = 3;

        public static DateOnly AdvanceNextDueDate(DateOnly current, int frequencyId) => frequencyId switch
        {
            Weekly => current.AddDays(7),
            Monthly => current.AddMonths(1),
            Yearly => current.AddYears(1),
            _ => current.AddMonths(1)
        };
    }
}
