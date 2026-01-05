namespace Touir.ExpensesManager.Users.Models
{
    public class Application
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? UrlPath { get; set; }
        public string? ResetPasswordUrlPath { get; set; }

        public ICollection<Role> Roles { get; set; } = new List<Role>();
        public ICollection<RequestAccess> RequestAccesses { get; set; } = new List<RequestAccess>();
        public ICollection<AllowedOrigin> AllowedOrigins { get; set; } = new List<AllowedOrigin>();

    }
}
