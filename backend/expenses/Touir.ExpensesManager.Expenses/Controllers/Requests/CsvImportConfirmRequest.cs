namespace Touir.ExpensesManager.Expenses.Controllers.Requests
{
    public class CsvImportConfirmRequest
    {
        public required IEnumerable<CsvImportConfirmRowDto> Rows { get; set; }
    }

    public class CsvImportConfirmRowDto
    {
        public required decimal Amount { get; set; }
        public required int CurrencyId { get; set; }
        public required DateOnly Date { get; set; }
        public int? CategoryId { get; set; }
        public int? SubcategoryId { get; set; }
        public string? Description { get; set; }
        public string[]? TagNames { get; set; }
        public int[]? FamilyIds { get; set; }
    }
}
