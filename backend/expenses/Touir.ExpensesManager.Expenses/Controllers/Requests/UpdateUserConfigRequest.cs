namespace Touir.ExpensesManager.Expenses.Controllers.Requests
{
    public class UpdateUserConfigRequest
    {
        public int? DefaultCurrencyId { get; set; }
        public int? DefaultCategoryId { get; set; }
    }

    public class UpdateCsvColumnMappingRequest
    {
        public Dictionary<string, string>? Mapping { get; set; }
    }
}
