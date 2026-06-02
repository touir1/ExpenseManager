namespace Touir.ExpensesManager.Expenses.Controllers.Requests
{
    public class ValidateRowsRequest
    {
        public required IEnumerable<RawCsvRowDto> Rows { get; set; }
    }

    public class RawCsvRowDto
    {
        public int RowNumber { get; set; }
        public string? Date { get; set; }
        public string? Amount { get; set; }
        public string? CurrencyCode { get; set; }
        public string? Category { get; set; }
        public string? Subcategory { get; set; }
        public string? Description { get; set; }
        public string? Tags { get; set; }
        public string? Families { get; set; }
    }
}
