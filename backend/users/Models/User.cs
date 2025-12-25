namespace com.touir.expenses.Users.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public int? FamilyId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedById { get; set; }
        public User CreatedBy { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public int? LastUpdatedById { get; set; }
        public User LastUpdatedBy { get; set; }
        public bool IsEmailValidated { get; set; }
        public string EmailValidationHash { get; set; }
        public bool IsDisabled { get; set; }

        public Authentication Authentication { get; set; }
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
