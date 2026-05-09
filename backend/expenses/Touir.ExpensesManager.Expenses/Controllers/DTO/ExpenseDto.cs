namespace Touir.ExpensesManager.Expenses.Controllers.DTO
{
    public class ExpenseDto
    {
        public long Id { get; set; }
        public decimal Amount { get; set; }
        public CurrencyDto? Currency { get; set; }
        public DateOnly Date { get; set; }
        public SubcategoryDto? Category { get; set; }
        public SubcategoryDto? Subcategory { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedFrom { get; set; }
    }
}
