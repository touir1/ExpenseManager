namespace com.touir.expenses.Users.Infrastructure.Options
{
    public class PostgresOptions
    {
        public string Server { get; set; }
        public int? Port { get; set; }
        public string Database { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ConnectionString => string.Format("Server={0};Port={1};User Id={2};Password={3};Database={4}", Server ?? "127.0.0.1", Port ?? 5432, UserName, Password, Database);

    }
}
