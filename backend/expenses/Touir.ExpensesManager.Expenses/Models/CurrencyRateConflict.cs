using Touir.ExpensesManager.Expenses.Models.Lookups;

namespace Touir.ExpensesManager.Expenses.Models
{
    public class CurrencyRateConflict
    {
        public int Id { get; set; }
        public int SourceCurrencyId { get; set; }
        public int DestinationCurrencyId { get; set; }
        public DateOnly Date { get; set; }
        public decimal AutomaticRate { get; set; }
        public decimal ManualRate { get; set; }
        public int StatusId { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public int? ResolvedById { get; set; }
        public int? ResolutionId { get; set; }
        public decimal? CustomRate { get; set; }

        public Currency SourceCurrency { get; set; } = null!;
        public Currency DestinationCurrency { get; set; } = null!;
        public ConflictStatus Status { get; set; } = null!;
        public ConflictResolution? Resolution { get; set; }
    }
}
