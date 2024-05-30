using com.touir.expenses.Users.Infrastructure.Contracts;
using com.touir.expenses.Users.Infrastructure.Options;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace com.touir.expenses.Users.Infrastructure
{
    public class CryptographyHelper : ICryptographyHelper
    {
        private readonly int? maximumSaltSize;
        public CryptographyHelper(IOptions<CryptographyOptions> cryptographyOptions) 
        {
            this.maximumSaltSize = cryptographyOptions.Value.MaximumSaltSize;
        }

        public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            var computedHash = GeneratePasswordHash(password, passwordSalt);
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != passwordHash[i])
                    return false;
            }
            return true;
        }

        public byte[] GeneratePasswordHash(string password, byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512(passwordSalt);
            return hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        public byte[] GenerateRandomSalt()
        {
            return RandomNumberGenerator.GetBytes(this.maximumSaltSize.Value);
        }
    }
}
