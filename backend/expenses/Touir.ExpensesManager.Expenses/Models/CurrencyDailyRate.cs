using Touir.ExpensesManager.Expenses.Models.Lookups;

namespace Touir.ExpensesManager.Expenses.Models
{
    public class CurrencyDailyRate
    {
        public int Id { get; set; }
        public int SourceCurrencyId { get; set; }
        public int DestinationCurrencyId { get; set; }
        public DateOnly Date { get; set; }
        public decimal Rate { get; set; }
        public int RateSourceId { get; set; }

        public Currency SourceCurrency { get; set; } = null!;
        public Currency DestinationCurrency { get; set; } = null!;
        public RateSource RateSource { get; set; } = null!;
    }
}
