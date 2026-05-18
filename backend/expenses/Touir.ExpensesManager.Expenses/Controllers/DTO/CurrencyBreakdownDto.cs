namespace Touir.ExpensesManager.Expenses.Controllers.DTO
{
    public class CurrencyBreakdownDto
    {
        public CurrencyDto Currency { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public decimal? ConvertedAmount { get; set; }
        public int ExpenseCount { get; set; }
    }
}
