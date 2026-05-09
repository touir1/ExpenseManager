namespace Touir.ExpensesManager.Expenses.Controllers.Requests
{
    public class CreateExpenseRequest
    {
        public decimal Amount { get; set; }
        public int CurrencyId { get; set; }
        public DateOnly Date { get; set; }
        public int? CategoryId { get; set; }
        public int? SubcategoryId { get; set; }
        public string? Description { get; set; }
    }
}
