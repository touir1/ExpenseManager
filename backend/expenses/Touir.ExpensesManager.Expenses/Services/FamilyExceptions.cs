namespace Touir.ExpensesManager.Expenses.Services
{
    public class FamilyNotFoundException : Exception
    {
        public FamilyNotFoundException(string message = ServiceErrors.FamilyNotFound) : base(message) { }
    }

    public class FamilyForbiddenException : Exception
    {
        public FamilyForbiddenException(string message = ServiceErrors.FamilyForbidden) : base(message) { }
    }

    public class FamilyConflictException : Exception
    {
        public FamilyConflictException(string message = ServiceErrors.FamilyAlreadyMember) : base(message) { }
    }

    public class FamilyInvitationException : Exception
    {
        public FamilyInvitationException(string message) : base(message) { }
    }
}
