namespace Touir.ExpensesManager.Users.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }

        public int ApplicationId { get; set; }
        public Application Application { get; set; }
        public ICollection<RoleRequestAccess> RoleRequestAccesses { get; set; } = new List<RoleRequestAccess>();
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
