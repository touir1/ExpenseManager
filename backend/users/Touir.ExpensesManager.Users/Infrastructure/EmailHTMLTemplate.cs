namespace Touir.ExpensesManager.Users.Infrastructure
{
    public sealed class EmailHTMLTemplate
    {
        public sealed class EmailVerification
        {
            public static readonly string Key = "EMAIL_VERIFICATION_TEMPLATE";
            public sealed class Variables
            {
                public static readonly string VerificationLink = "VERIFICATION_LINK";
                private Variables() { }
            }

            private EmailVerification() { }
        }

        private EmailHTMLTemplate() { }
    }
}
