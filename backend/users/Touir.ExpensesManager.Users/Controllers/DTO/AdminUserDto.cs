namespace Touir.ExpensesManager.Users.Controllers.DTO
{
    public class AdminUserDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsEmailValidated { get; set; }
        public DateTime CreatedAt { get; set; }
        public IEnumerable<RoleDto> Roles { get; set; } = [];
    }
}
