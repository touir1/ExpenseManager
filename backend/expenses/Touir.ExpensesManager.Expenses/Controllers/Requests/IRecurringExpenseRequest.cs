namespace Touir.ExpensesManager.Expenses.Controllers.Requests
{
    public interface IRecurringExpenseRequest
    {
        string Description { get; }
        decimal Amount { get; }
        int CurrencyId { get; }
        int CategoryId { get; }
        int? SubcategoryId { get; }
        int? FamilyId { get; }
        int FrequencyId { get; }
        DateOnly NextDueDate { get; }
        bool AutoCreate { get; }
    }
}
