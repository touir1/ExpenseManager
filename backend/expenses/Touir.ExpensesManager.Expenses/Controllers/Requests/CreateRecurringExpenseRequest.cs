namespace Touir.ExpensesManager.Expenses.Controllers.Requests
{
    public class CreateRecurringExpenseRequest : IRecurringExpenseRequest
    {
        public required string Description { get; set; }
        public required decimal Amount { get; set; }
        public required int CurrencyId { get; set; }
        public required int CategoryId { get; set; }
        public int? SubcategoryId { get; set; }
        public int? FamilyId { get; set; }
        public required int FrequencyId { get; set; }
        public required DateOnly NextDueDate { get; set; }
        public bool AutoCreate { get; set; }
    }
}
