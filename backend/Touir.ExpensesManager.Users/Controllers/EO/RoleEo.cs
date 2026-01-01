namespace Touir.ExpensesManager.Users.Controllers.EO
{
    public class RoleEo
    {
        public int? Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public ApplicationEo? Application { get; set; }
    }
}
