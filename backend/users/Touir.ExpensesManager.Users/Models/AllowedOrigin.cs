namespace Touir.ExpensesManager.Users.Models
{
    public class AllowedOrigin
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public bool IsGlobal { get; set; }

        public int? ApplicationId { get; set; }
        public Application? Application { get; set; }
    }
}
