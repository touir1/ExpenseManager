namespace Touir.ExpensesManager.Users.Infrastructure
{
    public static class EmailHTMLTemplate
    {
        public static class EmailVerification
        {
            public static readonly string Key = "EMAIL_VERIFICATION_TEMPLATE";
            public static class Variables
            {
                public static readonly string VerificationLink = "VERIFICATION_LINK";
            }
        }

        public static class PasswordReset
        {
            public static readonly string Key = "PASSWORD_RESET_TEMPLATE";
            public static class Variables
            {
                public static readonly string ResetLink = "RESET_LINK";
            }
        }
    }
}
