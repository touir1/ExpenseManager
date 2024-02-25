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
        public User CreatedBy { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public User LastUpdatedBy { get; set; }
        public bool IsDisabled { get; set; }
    }
}
