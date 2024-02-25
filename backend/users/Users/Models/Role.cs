namespace com.touir.expenses.Users.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Application Application { get; set; }
    }
}
