namespace com.touir.expenses.Users.Controllers.Requests
{
    public class ChangePasswordResetRequest
    {
        public string Email { get; set; }
        public string? VerificationHash { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }

    }
}
