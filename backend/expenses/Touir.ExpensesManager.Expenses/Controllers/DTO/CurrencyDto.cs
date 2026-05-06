namespace Touir.ExpensesManager.Expenses.Controllers.DTO
{
    public class CurrencyDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Symbol { get; set; } = null!;
        public int Decimals { get; set; }
    }
}
