using Touir.ExpensesManager.Expenses.Models.Lookups;

namespace Touir.ExpensesManager.Expenses.Models
{
    public class RecurringExpense
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Description { get; set; } = null!;
        public decimal Amount { get; set; }
        public int CurrencyId { get; set; }
        public int CategoryId { get; set; }
        public int? SubcategoryId { get; set; }
        public int? FamilyId { get; set; }
        public int FrequencyId { get; set; }
        public DateOnly NextDueDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public Currency? Currency { get; set; }
        public Category? Category { get; set; }
        public Category? Subcategory { get; set; }
        public Family? Family { get; set; }
        public RecurrenceFrequency? Frequency { get; set; }
    }
}
