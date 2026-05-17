namespace Touir.ExpensesManager.Expenses.Controllers.Requests
{
    public class AddRateRequest
    {
        public required int SourceCurrencyId { get; set; }
        public required int DestinationCurrencyId { get; set; }
        public required DateOnly Date { get; set; }
        public required decimal Rate { get; set; }
    }
}
