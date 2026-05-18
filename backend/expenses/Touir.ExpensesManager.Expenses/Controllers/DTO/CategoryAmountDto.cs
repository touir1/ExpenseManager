namespace Touir.ExpensesManager.Expenses.Controllers.DTO
{
    public class CategoryAmountDto
    {
        public SubcategoryDto? Category { get; set; }
        public decimal Amount { get; set; }
        public decimal? ConvertedAmount { get; set; }
    }
}
