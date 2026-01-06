using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Services;
using Touir.ExpensesManager.Users.Repositories.Contracts;
using Touir.ExpensesManager.Users.Infrastructure.Contracts;
using Touir.ExpensesManager.Users.Infrastructure.Options;
using Touir.ExpensesManager.Users.Tests.Repositories.Helpers;
using Microsoft.Extensions.Options;
using Moq;
using System.Text;

namespace Touir.ExpensesManager.Users.Tests.Services
{
    public class AuthenticationServiceTests
    {
        [Fact]
        public async Task AuthenticateAsync_ReturnsUser_WhenPasswordIsCorrect()
        {
            var user = new User { Id = 1, Email = "test@test.com" };
            var password = "password";
            var salt = Encoding.UTF8.GetBytes("somesaltstring1234");
            var hash = Encoding.UTF8.GetBytes("somehashstring1234");

            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync(user.Email)).ReturnsAsync(user);
            userRepo.Setup(r => r.GetUsedEmailValidationHashesAsync()).ReturnsAsync(new List<string>());
            userRepo.Setup(r => r.ValidateEmail(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            var authRepo = new Mock<IAuthenticationRepository>();
            authRepo.Setup(r => r.GetAuthenticationByUserIdAsync(user.Id)).ReturnsAsync(new Authentication { UserId = 1, User = user, HashPassword = "somehashstring1234", HashSalt = "somesaltstring1234" });

            var crypto = new Mock<ICryptographyHelper>();
            crypto.Setup(c => c.VerifyPasswordHash(password, It.IsAny<byte[]>(), It.IsAny<byte[]>())).Returns(true);

            var emailHelper = new Mock<IEmailHelper>();
            emailHelper.Setup(e => e.VerifyEmail(user.Email)).Returns(true);

            var appRepo = new Mock<IApplicationRepository>();
            var roleRepo = new Mock<IRoleRepository>();

            var jwtOptions = Options.Create(new JwtAuthOptions { SecretKey = "supersecretkey12345678901234567890", Issuer = "issuer", Audience = "audience", ExpiryInMinutes = 60 });
            var authServiceOptions = Options.Create(new AuthenticationServiceOptions { VerifyEmailBaseUrl = "http://localhost/verify" });

            var service = new AuthenticationService(jwtOptions, authServiceOptions, emailHelper.Object, crypto.Object, userRepo.Object, authRepo.Object, appRepo.Object, roleRepo.Object);
            var result = await service.AuthenticateAsync(user.Email, password);
            
            Assert.NotNull(result);
            Assert.Equal(user.Email, result.Email);
        }

        [Fact]
        public void GenerateJwtToken_ReturnsToken()
        {
            var jwtOptions = Options.Create(new JwtAuthOptions { SecretKey = "supersecretkey12345678901234567890", Issuer = "issuer", Audience = "audience", ExpiryInMinutes = 60 });
            var authServiceOptions = Options.Create(new AuthenticationServiceOptions { VerifyEmailBaseUrl = "http://localhost/verify" });
            var emailHelper = new Mock<IEmailHelper>();
            var crypto = new Mock<ICryptographyHelper>();
            var userRepo = new Mock<IUserRepository>();
            var authRepo = new Mock<IAuthenticationRepository>();
            var appRepo = new Mock<IApplicationRepository>();
            var roleRepo = new Mock<IRoleRepository>();
            
            var service = new AuthenticationService(jwtOptions, authServiceOptions, emailHelper.Object, crypto.Object, userRepo.Object, authRepo.Object, appRepo.Object, roleRepo.Object);
            var token = service.GenerateJwtToken(1, "test@test.com");
            
            Assert.False(string.IsNullOrWhiteSpace(token));
        }

        [Fact]
        public void ValidateToken_ReturnsValidResult()
        {
            var jwtOptions = Options.Create(new JwtAuthOptions { SecretKey = "supersecretkey12345678901234567890", Issuer = "issuer", Audience = "audience", ExpiryInMinutes = 60 });
            var authServiceOptions = Options.Create(new AuthenticationServiceOptions { VerifyEmailBaseUrl = "http://localhost/verify" });
            var emailHelper = new Mock<IEmailHelper>();
            var crypto = new Mock<ICryptographyHelper>();
            var userRepo = new Mock<IUserRepository>();
            var authRepo = new Mock<IAuthenticationRepository>();
            var appRepo = new Mock<IApplicationRepository>();
            var roleRepo = new Mock<IRoleRepository>();
            
            var service = new AuthenticationService(jwtOptions, authServiceOptions, emailHelper.Object, crypto.Object, userRepo.Object, authRepo.Object, appRepo.Object, roleRepo.Object);
            var token = service.GenerateJwtToken(1, "test@test.com");
            var result = service.ValidateToken(token);
            
            Assert.True(result.IsValid);
            Assert.NotNull(result.SecurityToken);
        }
    }
}
