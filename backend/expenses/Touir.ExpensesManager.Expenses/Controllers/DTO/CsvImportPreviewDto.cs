namespace Touir.ExpensesManager.Expenses.Controllers.DTO
{
    public class CsvImportPreviewDto
    {
        public int TotalRows { get; set; }
        public int ValidCount { get; set; }
        public int ErrorCount { get; set; }
        public IEnumerable<CsvImportRowPreviewDto> Rows { get; set; } = [];
    }

    public class CsvImportRowPreviewDto
    {
        public int RowNumber { get; set; }
        public bool IsValid { get; set; }
        public string[] Errors { get; set; } = [];
        // Raw display values from CSV
        public string? DateDisplay { get; set; }
        public string? AmountDisplay { get; set; }
        public string? CurrencyDisplay { get; set; }
        public string? CategoryDisplay { get; set; }
        public string? SubcategoryDisplay { get; set; }
        public string? DescriptionDisplay { get; set; }
        public string[]? TagNames { get; set; }
        // Resolved IDs (only when IsValid=true — sent back as confirm row)
        public DateOnly? Date { get; set; }
        public decimal? Amount { get; set; }
        public int? CurrencyId { get; set; }
        public int? CategoryId { get; set; }
        public int? SubcategoryId { get; set; }
        public int[]? FamilyIds { get; set; }
    }
}
