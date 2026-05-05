namespace Touir.ExpensesManager.Expenses.Models
{
    public class CurrencyPairDefault
    {
        public int SourceCurrencyId { get; set; }
        public int DestinationCurrencyId { get; set; }
        public decimal Rate { get; set; }

        public Currency SourceCurrency { get; set; } = null!;
        public Currency DestinationCurrency { get; set; } = null!;
    }
}
