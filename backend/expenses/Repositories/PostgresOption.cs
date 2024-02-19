namespace Expenses.Repositories
{
    public class PostgresOption
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string Database { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

    }
}
