namespace Touir.ExpensesManager.Expenses.Controllers.DTO
{
    public class ExpenseFilterDto
    {
        public DateOnly? DateFrom { get; set; }
        public DateOnly? DateTo { get; set; }
        public int? CategoryId { get; set; }
        public int? SubcategoryId { get; set; }
        public int? CurrencyId { get; set; }
        public decimal? AmountMin { get; set; }
        public decimal? AmountMax { get; set; }
        public string? Description { get; set; }
        public int[]? TagIds { get; set; }
        public int? FamilyId { get; set; }
        public int? DisplayCurrencyId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
