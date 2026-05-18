namespace Touir.ExpensesManager.Expenses.Controllers.DTO
{
    public class DashboardSummaryDto
    {
        public decimal TotalAmount { get; set; }
        public decimal? ConvertedTotal { get; set; }
        public CurrencyDto? DisplayCurrency { get; set; }
        public int ExpenseCount { get; set; }
        public decimal? PreviousPeriodTotal { get; set; }
        public decimal? ChangePercent { get; set; }
        public SubcategoryDto? TopCategory { get; set; }
        public decimal? TopCategoryAmount { get; set; }
    }
}
