using Touir.ExpensesManager.Users.Infrastructure.Options;
using Touir.ExpensesManager.Users.Services;
using Microsoft.Extensions.Options;

namespace Touir.ExpensesManager.Users.Tests.Services
{
    public class JwtTokenServiceTests
    {
        private static JwtTokenService CreateService() =>
            new JwtTokenService(Options.Create(new JwtAuthOptions
            {
                SecretKey = "supersecretkey12345678901234567890",
                Issuer = "issuer",
                Audience = "audience",
                ExpiryInMinutes = 60
            }));

        [Fact]
        public void GenerateJwtToken_ReturnsValidToken()
        {
            var service = CreateService();
            var token = service.GenerateJwtToken(1, "test@test.com", "John", "Doe");

            Assert.False(string.IsNullOrWhiteSpace(token));
        }

        [Fact]
        public void ValidateToken_ReturnsValid_ForValidToken()
        {
            var service = CreateService();
            var token = service.GenerateJwtToken(1, "test@test.com", "John", "Doe");
            var result = service.ValidateToken(token);

            Assert.True(result.IsValid);
            Assert.NotNull(result.SecurityToken);
            Assert.Null(result.Exception);
        }

        [Fact]
        public void ValidateToken_ReturnsInvalid_ForInvalidToken()
        {
            var service = CreateService();
            var invalidToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
            var result = service.ValidateToken(invalidToken);

            Assert.False(result.IsValid);
            Assert.NotNull(result.Exception);
        }
    }
}
