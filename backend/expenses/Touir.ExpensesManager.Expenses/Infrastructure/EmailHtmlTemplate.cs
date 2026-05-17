namespace Touir.ExpensesManager.Expenses.Infrastructure
{
    public static class EmailHtmlTemplate
    {
        public static class FamilyInvitation
        {
            public static readonly string Key = "FAMILY_INVITATION_TEMPLATE";
            public static class Variables
            {
                public static readonly string InviteLink = "INVITE_LINK";
                public static readonly string FamilyName = "FAMILY_NAME";
                public static readonly string InviterName = "INVITER_NAME";
            }
        }
    }
}
