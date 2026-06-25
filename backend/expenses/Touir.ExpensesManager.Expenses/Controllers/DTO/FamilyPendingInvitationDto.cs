namespace Touir.ExpensesManager.Expenses.Controllers.DTO
{
    public class FamilyPendingInvitationDto
    {
        public string Token { get; set; } = null!;
        public string InviteeEmail { get; set; } = null!;
        public DateTime InvitedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
