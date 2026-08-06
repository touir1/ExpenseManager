namespace Touir.ExpensesManager.Expenses.Controllers.DTO
{
    public class RecurringExpenseDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public CurrencyDto? Currency { get; set; }
        public SubcategoryDto? Category { get; set; }
        public SubcategoryDto? Subcategory { get; set; }
        public DateOnly NextDueDate { get; set; }
        public string Frequency { get; set; } = string.Empty;
    }
}
