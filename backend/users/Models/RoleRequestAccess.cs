namespace com.touir.expenses.Users.Models
{
    public class RoleRequestAccess
    {
        public int RoleId { get; set; }
        public Role Role { get; set; }
        public int RequestAccessId { get; set; }
        public RequestAccess RequestAccess { get; set; }
    }
}
