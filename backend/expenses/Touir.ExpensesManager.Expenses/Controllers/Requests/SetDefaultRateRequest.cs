namespace Touir.ExpensesManager.Expenses.Controllers.Requests
{
    public class SetDefaultRateRequest
    {
        public required int SourceCurrencyId { get; set; }
        public required int DestinationCurrencyId { get; set; }
        public required decimal Rate { get; set; }
    }
}
