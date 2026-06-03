namespace Touir.ExpensesManager.Expenses.Services
{
    internal static class ServiceErrors
    {
        // Family – name
        public const string FamilyNameAlreadyExists = "FAMILY_NAME_ALREADY_EXISTS";
        // Family – membership
        public const string FamilyNotFound = "FAMILY_NOT_FOUND";
        public const string UserNotFound = "USER_NOT_FOUND";
        public const string FamilyNotMember = "FAMILY_NOT_MEMBER";
        public const string FamilyAlreadyMember = "FAMILY_ALREADY_MEMBER";
        // Family – forbidden operations
        public const string FamilyForbidden = "FAMILY_FORBIDDEN";
        public const string FamilyCannotInviteDefault = "FAMILY_CANNOT_INVITE_DEFAULT";
        public const string FamilyCannotRemoveDefault = "FAMILY_CANNOT_REMOVE_DEFAULT";
        public const string FamilyCannotRemoveSelfHead = "FAMILY_CANNOT_REMOVE_SELF_HEAD";
        public const string FamilyCannotChangeOwnRole = "FAMILY_CANNOT_CHANGE_OWN_ROLE";
        public const string FamilyCannotArchiveDefault = "FAMILY_CANNOT_ARCHIVE_DEFAULT";
        public const string FamilyCannotLeaveDefault = "FAMILY_CANNOT_LEAVE_DEFAULT";
        public const string FamilyCannotLeaveLastHead = "FAMILY_CANNOT_LEAVE_LAST_HEAD";
        // Tags
        public const string TagNotVisible = "TAG_NOT_VISIBLE";
        // Invitations
        public const string FamilyInvitationInvalid = "FAMILY_INVITATION_INVALID";
        public const string FamilyInvitationAlreadyAccepted = "FAMILY_INVITATION_ALREADY_ACCEPTED";
        public const string FamilyInvitationExpired = "FAMILY_INVITATION_EXPIRED";
    }
}
