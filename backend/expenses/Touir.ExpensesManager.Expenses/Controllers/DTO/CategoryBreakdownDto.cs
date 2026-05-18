namespace Touir.ExpensesManager.Expenses.Controllers.DTO
{
    public class CategoryBreakdownDto
    {
        public SubcategoryDto? Category { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? ConvertedTotal { get; set; }
        public decimal Percentage { get; set; }
        public IEnumerable<CategoryAmountDto> Subcategories { get; set; } = [];
    }
}
