namespace Touir.ExpensesManager.Expenses.Controllers.DTO
{
    public class RecurringExpenseDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int CurrencyId { get; set; }
        public CurrencyDto? Currency { get; set; }
        public int CategoryId { get; set; }
        public SubcategoryDto? Category { get; set; }
        public int? SubcategoryId { get; set; }
        public SubcategoryDto? Subcategory { get; set; }
        public int? FamilyId { get; set; }
        public int FrequencyId { get; set; }
        public DateOnly NextDueDate { get; set; }
        public string Frequency { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool AutoCreate { get; set; }
    }
}
