namespace Touir.ExpensesManager.Users.Controllers.Responses
{
    public class SessionResponse
    {
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
