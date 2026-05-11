namespace Touir.ExpensesManager.Users.Controllers
{
    internal static class ControllerErrors
    {
        public const string ServerError = "SERVER_ERROR";
        public const string MissingParameters = "MISSING_PARAMETERS";
        public const string InvalidUsernameOrPassword = "INVALID_USERNAME_OR_PASSWORD";
        public const string NoAssignedRole = "NO_ASSIGNED_ROLE";
        public const string MissingToken = "MISSING_TOKEN";
        public const string InvalidToken = "INVALID_TOKEN";
        public const string UserNotFound = "USER_NOT_FOUND";
        public const string EmailVerificationFailed = "EMAIL_VERIFICATION_FAILED";
        public const string SetNewPasswordFailed = "SET_NEW_PASSWORD_FAILED";
        public const string RequestPasswordResetFailed = "REQUEST_PASSWORD_RESET_FAILED";
        public const string CreatePasswordFailed = "CREATE_PASSWORD_FAILED";
        public const string ResetPasswordFailed = "RESET_PASSWORD_FAILED";
    }
}
