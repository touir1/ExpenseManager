using System.Diagnostics.CodeAnalysis;

namespace Touir.ExpensesManager.Expenses.Infrastructure.Options
{
    [ExcludeFromCodeCoverage]
    public class RecurringExpenseOptions
    {
        public TimeOnly GenerationTime { get; set; } = new(3, 0);
    }
}
