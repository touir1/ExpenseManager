namespace Touir.ExpensesManager.Expenses.Controllers.Requests
{
    public class UpdateExpenseRequest : IExpenseRequest
    {
        public required decimal Amount { get; set; }
        public required int CurrencyId { get; set; }
        public required DateOnly Date { get; set; }
        public int? CategoryId { get; set; }
        public int? SubcategoryId { get; set; }
        public string? Description { get; set; }
    }
}
