namespace Touir.ExpensesManager.Expenses.Controllers.Requests
{
    public class AdminCategoryRequest
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
    }
}
