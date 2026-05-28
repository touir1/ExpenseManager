namespace Touir.ExpensesManager.Expenses.Controllers.DTO
{
    public class PagedRatesResponse
    {
        public IEnumerable<RateDto> Rates { get; set; } = [];
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
