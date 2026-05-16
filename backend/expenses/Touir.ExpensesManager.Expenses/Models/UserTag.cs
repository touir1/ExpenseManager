using Touir.ExpensesManager.Expenses.Models.External;

namespace Touir.ExpensesManager.Expenses.Models
{
    public class UserTag
    {
        public int UserId { get; set; }
        public int TagId { get; set; }

        public User User { get; set; } = null!;
        public Tag Tag { get; set; } = null!;
    }
}
