namespace Touir.ExpensesManager.Users.Controllers.DTO
{
    public class RoleDto
    {
        public int? Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public ApplicationDto? Application { get; set; }
    }
}
