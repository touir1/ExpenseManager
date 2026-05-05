using Touir.ExpensesManager.Expenses.Models.External;
using Touir.ExpensesManager.Expenses.Models.Lookups;

namespace Touir.ExpensesManager.Expenses.Models
{
    public class FamilyMembership
    {
        public int Id { get; set; }
        public int FamilyId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public DateTime JoinedAt { get; set; }

        public Family Family { get; set; } = null!;
        public User User { get; set; } = null!;
        public FamilyRole Role { get; set; } = null!;
    }
}
