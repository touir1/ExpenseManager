namespace Touir.ExpensesManager.Expenses.Controllers.Requests
{
    public class AdminUpdateCurrencyRequest
    {
        public required string Name { get; set; }
        public required string Symbol { get; set; }
        public int Decimals { get; set; } = 2;
    }
}
