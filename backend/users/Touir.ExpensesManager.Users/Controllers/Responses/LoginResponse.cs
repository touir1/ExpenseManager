using Touir.ExpensesManager.Users.Controllers.DTO;

namespace Touir.ExpensesManager.Users.Controllers.Responses
{
    public class LoginResponse
    {
        public UserDto? User { get; set; }
        public IEnumerable<RoleDto>? Roles { get; set; }
    }
}
