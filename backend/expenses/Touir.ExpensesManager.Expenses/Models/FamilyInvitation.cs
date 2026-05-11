namespace Touir.ExpensesManager.Expenses.Models
{
    public class FamilyInvitation
    {
        public int Id { get; set; }
        public int FamilyId { get; set; }
        public string InviteeEmail { get; set; } = null!;
        public string Token { get; set; } = null!;
        public int InvitedById { get; set; }
        public DateTime InvitedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public int? AcceptedByUserId { get; set; }

        public Family Family { get; set; } = null!;
    }
}
