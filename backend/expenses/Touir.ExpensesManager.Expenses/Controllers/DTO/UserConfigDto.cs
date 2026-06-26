namespace Touir.ExpensesManager.Expenses.Controllers.DTO
{
    public class UserConfigDto
    {
        public int? DefaultCurrencyId { get; set; }
        public CurrencyDto? DefaultCurrency { get; set; }
        public int? DefaultCategoryId { get; set; }
    }
}
