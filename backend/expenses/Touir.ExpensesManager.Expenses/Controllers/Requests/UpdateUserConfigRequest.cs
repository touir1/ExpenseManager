namespace Touir.ExpensesManager.Expenses.Controllers.Requests
{
    public class UpdateUserConfigRequest
    {
        public int? DefaultCurrencyId { get; set; }
        public int? DefaultCategoryId { get; set; }
    }
}
