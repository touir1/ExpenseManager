namespace Touir.ExpensesManager.Users.Infrastructure.Options
{
    public class AuthenticationServiceOptions
    {
        public string VerifyEmailBaseUrl { get; set; }
        public string ResetPasswordBaseUrl { get; set; }
        public int EmailVerificationExpiryInHours { get; set; }
        public int PasswordResetExpiryInHours { get; set; }
    }
}
