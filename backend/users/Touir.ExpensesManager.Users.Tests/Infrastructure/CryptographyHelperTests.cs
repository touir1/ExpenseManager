using Microsoft.Extensions.Options;
using Touir.ExpensesManager.Users.Infrastructure;
using Touir.ExpensesManager.Users.Infrastructure.Options;
using System.Text;

namespace Touir.ExpensesManager.Users.Tests.Infrastructure
{
    public class CryptographyHelperTests
    {
        [Fact]
        public void GeneratePasswordHash_And_VerifyPasswordHash_Works()
        {
            var options = Options.Create(new CryptographyOptions { MaximumSaltSize = 16 });
            var helper = new CryptographyHelper(options);
            var password = "TestPassword123!";
            
            var salt = helper.GenerateRandomSalt();
            var hash = helper.GeneratePasswordHash(password, salt);
            
            Assert.True(helper.VerifyPasswordHash(password, hash, salt));
            Assert.False(helper.VerifyPasswordHash("WrongPassword", hash, salt));
        }

        [Fact]
        public void GenerateRandomSalt_ReturnsDifferentValues()
        {
            var options = Options.Create(new CryptographyOptions { MaximumSaltSize = 16 });
            var helper = new CryptographyHelper(options);
            
            var salt1 = helper.GenerateRandomSalt();
            var salt2 = helper.GenerateRandomSalt();
            
            Assert.NotEqual(Encoding.UTF8.GetString(salt1), Encoding.UTF8.GetString(salt2));
            Assert.Equal(24, salt1.Length); // 16 bytes base64 = 24 chars
        }
    }
}
