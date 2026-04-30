namespace Touir.ExpensesManager.Users.Controllers.Requests
{
    public class ChangePasswordRequest
    {
        public string Email { get; set; }
        public string? OldPassword { get; set; }
        public string NewPassword { get; set; }

    }
}
