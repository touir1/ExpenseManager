using com.touir.expenses.Users.Controllers.EO;

namespace com.touir.expenses.Users.Controllers.Responses
{
    public class LoginResponse
    {
        public UserEo? User { get; set; }
        public IEnumerable<RoleEo>? Roles { get; set; }
        public string? Token { get; set; }
    }
}
