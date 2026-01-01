using Touir.ExpensesManager.Users.Controllers.EO;

namespace Touir.ExpensesManager.Users.Controllers.Responses
{
    public class LoginResponse
    {
        public UserEo? User { get; set; }
        public IEnumerable<RoleEo>? Roles { get; set; }
        public string? Token { get; set; }
    }
}
