namespace Touir.ExpensesManager.Users.Controllers.Requests
{
    public class SetUserRolesRequest
    {
        public required IEnumerable<int> RoleIds { get; set; }
    }
}
