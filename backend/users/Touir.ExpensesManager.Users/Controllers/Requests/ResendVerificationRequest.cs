namespace Touir.ExpensesManager.Users.Controllers.Requests
{
    public class ResendVerificationRequest
    {
        public required string Email { get; set; }
        public required string ApplicationCode { get; set; }
    }
}
