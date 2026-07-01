namespace Touir.ExpensesManager.Expenses.Controllers.DTO
{
    public class CsvHeaderDetectionDto
    {
        public string[] RawHeaders { get; set; } = [];
        public Dictionary<string, string> SuggestedMapping { get; set; } = new();
        public bool HeadersMatchExactly { get; set; }
    }
}
