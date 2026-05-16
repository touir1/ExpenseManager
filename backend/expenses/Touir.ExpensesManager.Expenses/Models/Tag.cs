namespace Touir.ExpensesManager.Expenses.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public ICollection<UserTag> UserTags { get; set; } = [];
        public ICollection<ExpenseTag> ExpenseTags { get; set; } = [];
    }
}
