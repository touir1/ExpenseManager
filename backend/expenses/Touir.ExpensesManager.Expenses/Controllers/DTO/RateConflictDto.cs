namespace Touir.ExpensesManager.Expenses.Controllers.DTO
{
    public class RateConflictDto
    {
        public int Id { get; set; }
        public int SourceCurrencyId { get; set; }
        public int DestinationCurrencyId { get; set; }
        public DateOnly Date { get; set; }
        public decimal AutoRate { get; set; }
        public decimal ManualRate { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? ResolvedAt { get; set; }
    }
}
