namespace Touir.ExpensesManager.Expenses.Controllers.Requests
{
    public class RefreshRatesRequest
    {
        public required DateOnly From { get; set; }
        public int? SourceCurrencyId { get; set; }
        public int? DestinationCurrencyId { get; set; }
    }
}
