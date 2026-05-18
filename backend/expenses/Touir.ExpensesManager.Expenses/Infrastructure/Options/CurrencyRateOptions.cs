using System.Diagnostics.CodeAnalysis;

namespace Touir.ExpensesManager.Expenses.Infrastructure.Options
{
    [ExcludeFromCodeCoverage]
    public class CurrencyRateOptions
    {
        public TimeOnly UpdateTime { get; set; } = new(2, 0);
    }
}
