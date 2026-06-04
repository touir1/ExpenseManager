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
    }
}
