namespace Touir.ExpensesManager.Expenses.Controllers.DTO
{
    public class MonthlyBreakdownDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? ConvertedTotal { get; set; }
        public IEnumerable<CategoryAmountDto> ByCategory { get; set; } = [];
    }
}
