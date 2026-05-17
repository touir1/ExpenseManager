namespace Touir.ExpensesManager.Expenses.Infrastructure.Options
{
    public class FamilyOptions
    {
        public int InviteExpiryInDays { get; set; }
        public string InviteBaseUrl { get; set; } = "https://localhost/families/accept-invite";
    }
}
