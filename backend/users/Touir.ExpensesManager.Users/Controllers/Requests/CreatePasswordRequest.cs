namespace Touir.ExpensesManager.Users.Controllers.Requests
{
    public class CreatePasswordRequest
    {
        public string Email { get; set; }
        public string? VerificationHash { get; set; }
        public string NewPassword { get; set; }
    }
}
