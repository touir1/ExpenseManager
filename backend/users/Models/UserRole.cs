namespace com.touir.expenses.Users.Models
{
    public class UserRole
    {
        public int UserId { get; set; }
        public User User { get; set; }
        public int RoleId { get; set; }
        public Role Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedById { get; set; }
        public User CreatedBy { get; set; }
    }
}
