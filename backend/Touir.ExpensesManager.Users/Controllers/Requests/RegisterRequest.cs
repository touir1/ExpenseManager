namespace Touir.ExpensesManager.Users.Controllers.Requests
{
    public class RegisterRequest
    {
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public string Email { get; set; }
        public string? ApplicationCode { get; set; }
    }
}
