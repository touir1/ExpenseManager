namespace com.touir.expenses.Users.Controllers.Requests
{
    public class ChangePasswordRequest
    {
        public string Email { get; set; }
        public string? VerificationHash { get; set; }
        public string? OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }

    }
}
