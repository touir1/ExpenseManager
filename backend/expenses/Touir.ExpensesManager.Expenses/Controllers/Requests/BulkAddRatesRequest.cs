namespace Touir.ExpensesManager.Expenses.Controllers.Requests
{
    public class BulkAddRatesRequest
    {
        public required List<AddRateRequest> Rates { get; set; }
    }
}
