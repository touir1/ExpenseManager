using System.Text;

namespace com.touir.expenses.Users.Models
{
    public class Authentication
    {
        public User User { get; set; }
        public string HashPassword { get; set; }
        public string HashSalt { get; set; }
        public bool IsTemporaryPassword { get; set; }

        public byte[] HashPasswordBytes => Encoding.UTF8.GetBytes(HashPassword);
        public byte[] HashSaltBytes => Encoding.UTF8.GetBytes(HashSalt);
    }
}
