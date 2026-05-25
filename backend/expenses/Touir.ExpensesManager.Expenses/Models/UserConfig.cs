namespace Touir.ExpensesManager.Expenses.Models
{
    public class UserConfig
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? DefaultCurrencyId { get; set; }

        public Currency? DefaultCurrency { get; set; }
    }
}
