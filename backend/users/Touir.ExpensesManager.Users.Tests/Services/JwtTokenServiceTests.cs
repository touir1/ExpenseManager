using Touir.ExpensesManager.Users.Infrastructure.Options;
using Touir.ExpensesManager.Users.Services;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;

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

        [Fact]
        public void GenerateJwtToken_ReturnsValidToken_WhenFirstNameIsNull()
        {
            var service = CreateService();
            var token = service.GenerateJwtToken(1, "test@test.com", null, "Doe");

            Assert.False(string.IsNullOrWhiteSpace(token));
            var result = service.ValidateToken(token);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void GenerateJwtToken_ReturnsValidToken_WhenLastNameIsNull()
        {
            var service = CreateService();
            var token = service.GenerateJwtToken(1, "test@test.com", "John", null);

            Assert.False(string.IsNullOrWhiteSpace(token));
            var result = service.ValidateToken(token);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void GenerateJwtToken_ReturnsValidToken_WhenBothNamesAreNull()
        {
            var service = CreateService();
            var token = service.GenerateJwtToken(42, "noname@test.com", null, null);

            Assert.False(string.IsNullOrWhiteSpace(token));
            var result = service.ValidateToken(token);
            Assert.True(result.IsValid);
        }

        [Fact]
        public void GenerateJwtToken_ContainsIsAdminTrue_WhenAdminUser()
        {
            var service = CreateService();
            var token = service.GenerateJwtToken(1, "admin@test.com", "Admin", "User", isAdmin: true);

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var claim = jwt.Claims.FirstOrDefault(c => c.Type == "isAdmin");
            Assert.NotNull(claim);
            Assert.Equal("true", claim.Value);
        }

        [Fact]
        public void GenerateJwtToken_ContainsIsAdminFalse_WhenNonAdminUser()
        {
            var service = CreateService();
            var token = service.GenerateJwtToken(2, "user@test.com", "User", "One", isAdmin: false);

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var claim = jwt.Claims.FirstOrDefault(c => c.Type == "isAdmin");
            Assert.NotNull(claim);
            Assert.Equal("false", claim.Value);
        }

        [Fact]
        public void GenerateJwtToken_DefaultsIsAdminToFalse()
        {
            var service = CreateService();
            // call without isAdmin param — default should be false
            var token = service.GenerateJwtToken(3, "regular@test.com", "R", "U");

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var claim = jwt.Claims.FirstOrDefault(c => c.Type == "isAdmin");
            Assert.NotNull(claim);
            Assert.Equal("false", claim.Value);
        }
    }
}
