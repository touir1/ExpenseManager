namespace Touir.ExpensesManager.Notifications.Infrastructure
{
    public static class EmailHtmlTemplate
    {
        public static class FamilyMemberRemoved
        {
            public static readonly string Key = "FAMILY_MEMBER_REMOVED_TEMPLATE";
            public static class Variables
            {
                public static readonly string FamilyName = "FAMILY_NAME";
                public static readonly string RemovedByName = "REMOVED_BY_NAME";
                public static readonly string ExpenseCount = "EXPENSE_COUNT";
            }
        }

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

        public static class PasswordChanged
        {
            public static readonly string Key = "PASSWORD_CHANGED_TEMPLATE";
            public static class Variables
            {
                public static readonly string FirstName = "FIRST_NAME";
            }
        }

        public static class FamilyInvitation
        {
            public static readonly string Key = "FAMILY_INVITATION_TEMPLATE";
            public static class Variables
            {
                public static readonly string InviterName = "INVITER_NAME";
                public static readonly string FamilyName = "FAMILY_NAME";
                public static readonly string InviteLink = "INVITE_LINK";
            }
        }

        public static class FamilyInvitationAccepted
        {
            public static readonly string Key = "FAMILY_INVITATION_ACCEPTED_TEMPLATE";
            public static class Variables
            {
                public static readonly string AcceptorName = "ACCEPTOR_NAME";
                public static readonly string FamilyName = "FAMILY_NAME";
            }
        }
    }
}
