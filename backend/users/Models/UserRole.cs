namespace com.touir.expenses.Users.Models
{
    public class UserRole
    {
        public User User { get; set; }
        public Role Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public User CreatedBy { get; set; }
    }
}
