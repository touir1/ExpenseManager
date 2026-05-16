namespace Touir.ExpensesManager.Expenses.Controllers.Requests
{
    public interface IExpenseRequest
    {
        decimal Amount { get; }
        int CurrencyId { get; }
        DateOnly Date { get; }
        int? CategoryId { get; }
        int? SubcategoryId { get; }
        string? Description { get; }
        int[]? TagIds { get; }
    }
}
