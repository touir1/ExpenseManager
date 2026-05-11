namespace Touir.ExpensesManager.Expenses.Services
{
    public class FamilyNotFoundException : Exception
    {
        public FamilyNotFoundException(string message = "FAMILY_NOT_FOUND") : base(message) { }
    }

    public class FamilyForbiddenException : Exception
    {
        public FamilyForbiddenException(string message = "FAMILY_FORBIDDEN") : base(message) { }
    }

    public class FamilyConflictException : Exception
    {
        public FamilyConflictException(string message = "FAMILY_ALREADY_MEMBER") : base(message) { }
    }

    public class FamilyInvitationException : Exception
    {
        public FamilyInvitationException(string message) : base(message) { }
    }
}
