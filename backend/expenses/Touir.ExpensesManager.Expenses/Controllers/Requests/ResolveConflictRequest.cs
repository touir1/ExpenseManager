namespace Touir.ExpensesManager.Expenses.Controllers.Requests
{
    public class ResolveConflictRequest
    {
        public required string Resolution { get; set; }
        public decimal? CustomRate { get; set; }
    }
}
