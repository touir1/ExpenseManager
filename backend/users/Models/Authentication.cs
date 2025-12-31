using System.Text;

namespace com.touir.expenses.Users.Models
{
    public class Authentication
    {
        public int UserId { get; set; }
        public User User { get; set; }
        public string HashPassword { get; set; }
        public string HashSalt { get; set; }
        public bool IsTemporaryPassword { get; set; }
        public string? PasswordResetHash { get; set; }
        public DateTime? PasswordResetRequestedAt { get; set; }

        public byte[] HashPasswordBytes {
            get => Encoding.UTF8.GetBytes(HashPassword);
            set => HashPassword = Encoding.UTF8.GetString(value);
        }
        public byte[] HashSaltBytes
        {
            get => Encoding.UTF8.GetBytes(HashSalt);
            set => HashSalt = Encoding.UTF8.GetString(value);
        }
    }
}
