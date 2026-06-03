namespace Touir.ExpensesManager.Expenses.Controllers
{
    internal static class ControllerErrors
    {
        public const string ServerError = "SERVER_ERROR";
        public const string MissingUser = "UNAUTHORIZED";
        public const string ExpenseNotFound = "EXPENSE_NOT_FOUND";
        public const string MissingParameters = "MISSING_PARAMETERS";
        public const string TagNotFound = "TAG_NOT_FOUND";
        public const string InvalidMonth = "INVALID_MONTH";
        public const string ImportNoFile = "IMPORT_NO_FILE";
        public const string ImportFileTooLarge = "IMPORT_FILE_TOO_LARGE";
        public const string InvalidFileType = "INVALID_FILE_TYPE";
        public const string InvalidFileContent = "INVALID_FILE_CONTENT";
        public const string ImportTimeout = "IMPORT_TIMEOUT";
    }
}
