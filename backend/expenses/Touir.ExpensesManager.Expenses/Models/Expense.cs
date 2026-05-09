using Touir.ExpensesManager.Expenses.Models.External;
using Touir.ExpensesManager.Expenses.Models.Lookups;

namespace Touir.ExpensesManager.Expenses.Models
{
    public class Expense
    {
        public long Id { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public int CurrencyId { get; set; }
        public DateOnly Date { get; set; }
        public int? CategoryId { get; set; }
        public int? SubcategoryId { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedById { get; set; }
        public int CreatedFromId { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public int? ModifiedById { get; set; }
        public int? ModifiedFromId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        public User User { get; set; } = null!;
        public Currency Currency { get; set; } = null!;
        public Category? Category { get; set; }
        public Category? Subcategory { get; set; }
        public OperationSource CreatedFrom { get; set; } = null!;
        public ModifiedSource? ModifiedFrom { get; set; }
    }
}
