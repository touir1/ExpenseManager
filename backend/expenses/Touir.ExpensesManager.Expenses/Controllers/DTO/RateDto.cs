namespace Touir.ExpensesManager.Expenses.Controllers.DTO
{
    public class RateDto
    {
        public int Id { get; set; }
        public int SourceCurrencyId { get; set; }
        public int DestinationCurrencyId { get; set; }
        public DateOnly Date { get; set; }
        public decimal Rate { get; set; }
        public string RateSource { get; set; } = null!;
    }
}
