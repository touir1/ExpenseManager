using Touir.ExpensesManager.Users.Infrastructure.Contracts;
using Touir.ExpensesManager.Users.Infrastructure.Options;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Touir.ExpensesManager.Users.Infrastructure
{
    public class CryptographyHelper : ICryptographyHelper
    {
        private readonly int? _maximumSaltSize;
        public CryptographyHelper(IOptions<CryptographyOptions> cryptographyOptions) 
        {
            this._maximumSaltSize = cryptographyOptions.Value.MaximumSaltSize;
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
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            var hashBase64 = Convert.ToBase64String(hash);
            return Encoding.UTF8.GetBytes(hashBase64);
        }

        public byte[] GenerateRandomSalt()
        {
            // Generate a random salt as bytes, then encode as Base64 and return as UTF8 bytes
            var saltBytes = RandomNumberGenerator.GetBytes(this._maximumSaltSize.Value);
            var saltBase64 = Convert.ToBase64String(saltBytes);
            return Encoding.UTF8.GetBytes(saltBase64);
        }
    }
}
