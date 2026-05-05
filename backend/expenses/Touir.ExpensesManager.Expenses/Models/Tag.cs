using Touir.ExpensesManager.Expenses.Models.External;

namespace Touir.ExpensesManager.Expenses.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int UserId { get; set; }

        public User User { get; set; } = null!;
    }
}
