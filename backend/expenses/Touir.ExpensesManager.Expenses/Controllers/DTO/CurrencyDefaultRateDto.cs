namespace Touir.ExpensesManager.Expenses.Controllers.DTO
{
    public class CurrencyDefaultRateDto
    {
        public int DestinationCurrencyId { get; set; }
        public string DestinationCode { get; set; } = null!;
        public decimal? DefaultRate { get; set; }
        public decimal? LastAutoRate { get; set; }
        public string? LastAutoRateDate { get; set; }
    }
}
