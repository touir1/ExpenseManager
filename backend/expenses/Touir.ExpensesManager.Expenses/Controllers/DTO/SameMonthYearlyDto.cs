namespace Touir.ExpensesManager.Expenses.Controllers.DTO
{
    public class SameMonthYearlyDto
    {
        public int Year { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? ConvertedTotal { get; set; }
    }
}
