namespace Touir.ExpensesManager.Users.Infrastructure.Options
{
    public class JwtAuthOptions
    {
        public string SecretKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int ExpiryInMinutes { get; set; }
    }
}
