using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Services;
using Touir.ExpensesManager.Users.Repositories.Contracts;
using Touir.ExpensesManager.Users.Infrastructure.Contracts;
using Touir.ExpensesManager.Users.Infrastructure.Options;
using Touir.ExpensesManager.Users.Controllers.EO;
using Microsoft.Extensions.Options;
using Moq;
using System.Text;

namespace Touir.ExpensesManager.Users.Tests.Services
{
    public class AuthenticationServiceTests
    {
        private Mock<IEmailHelper> CreateEmailHelperMock()
        {
            var mock = new Mock<IEmailHelper>();
            mock.Setup(e => e.VerifyEmail(It.IsAny<string>())).Returns(true);
            mock.Setup(e => e.GetEmailTemplate(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).Returns("<html></html>");
            mock.Setup(e => e.SendEmail(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<ICollection<string>>())).Returns(true);
            return mock;
        }

        private AuthenticationService CreateService(
            Mock<IUserRepository>? userRepo = null,
            Mock<IAuthenticationRepository>? authRepo = null,
            Mock<IApplicationRepository>? appRepo = null,
            Mock<IRoleRepository>? roleRepo = null,
            Mock<ICryptographyHelper>? crypto = null,
            Mock<IEmailHelper>? emailHelper = null)
        {
            var jwtOptions = Options.Create(new JwtAuthOptions 
            { 
                SecretKey = "supersecretkey12345678901234567890", 
                Issuer = "issuer", 
                Audience = "audience", 
                ExpiryInMinutes = 60 
            });
            var authServiceOptions = Options.Create(new AuthenticationServiceOptions 
            { 
                VerifyEmailBaseUrl = "http://localhost/verify" 
            });

            return new AuthenticationService(
                jwtOptions,
                authServiceOptions,
                emailHelper?.Object ?? CreateEmailHelperMock().Object,
                crypto?.Object ?? new Mock<ICryptographyHelper>().Object,
                userRepo?.Object ?? new Mock<IUserRepository>().Object,
                authRepo?.Object ?? new Mock<IAuthenticationRepository>().Object,
                appRepo?.Object ?? new Mock<IApplicationRepository>().Object,
                roleRepo?.Object ?? new Mock<IRoleRepository>().Object
            );
        }

        #region AuthenticateAsync Tests

        [Fact]
        public async Task AuthenticateAsync_ReturnsUser_WhenPasswordIsCorrect()
        {
            var user = new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "test@test.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var auth = new Authentication { UserId = 1, User = user, HashPassword = "hash", HashSalt = "salt" };

            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync(user);

            var authRepo = new Mock<IAuthenticationRepository>();
            authRepo.Setup(r => r.GetAuthenticationByUserIdAsync(1)).ReturnsAsync(auth);

            var crypto = new Mock<ICryptographyHelper>();
            crypto.Setup(c => c.VerifyPasswordHash("password", It.IsAny<byte[]>(), It.IsAny<byte[]>())).Returns(true);

            var service = CreateService(userRepo, authRepo, crypto: crypto);
            var result = await service.AuthenticateAsync("test@test.com", "password");

            Assert.NotNull(result);
            Assert.Equal("test@test.com", result.Email);
            Assert.Equal("John", result.FirstName);
            Assert.Equal("Doe", result.LastName);
        }

        [Fact]
        public async Task AuthenticateAsync_ReturnsNull_WhenUserNotFound()
        {
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

            var service = CreateService(userRepo);
            var result = await service.AuthenticateAsync("test@test.com", "password");

            Assert.Null(result);
        }

        [Fact]
        public async Task AuthenticateAsync_ReturnsNull_WhenAuthenticationNotFound()
        {
            var user = new User { Id = 1, Email = "test@test.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync(user);

            var authRepo = new Mock<IAuthenticationRepository>();
            authRepo.Setup(r => r.GetAuthenticationByUserIdAsync(1)).ReturnsAsync((Authentication?)null);

            var service = CreateService(userRepo, authRepo);
            var result = await service.AuthenticateAsync("test@test.com", "password");

            Assert.Null(result);
        }

        [Fact]
        public async Task AuthenticateAsync_ReturnsNull_WhenPasswordIsIncorrect()
        {
            var user = new User { Id = 1, Email = "test@test.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var auth = new Authentication { UserId = 1, User = user, HashPassword = "hash", HashSalt = "salt" };

            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync(user);

            var authRepo = new Mock<IAuthenticationRepository>();
            authRepo.Setup(r => r.GetAuthenticationByUserIdAsync(1)).ReturnsAsync(auth);

            var crypto = new Mock<ICryptographyHelper>();
            crypto.Setup(c => c.VerifyPasswordHash("wrongpassword", It.IsAny<byte[]>(), It.IsAny<byte[]>())).Returns(false);

            var service = CreateService(userRepo, authRepo, crypto: crypto);
            var result = await service.AuthenticateAsync("test@test.com", "wrongpassword");

            Assert.Null(result);
        }

        #endregion

        #region JWT Token Tests

        [Fact]
        public void GenerateJwtToken_ReturnsValidToken()
        {
            var service = CreateService();
            var token = service.GenerateJwtToken(1, "test@test.com");

            Assert.False(string.IsNullOrWhiteSpace(token));
        }

        [Fact]
        public void ValidateToken_ReturnsValid_ForValidToken()
        {
            var service = CreateService();
            var token = service.GenerateJwtToken(1, "test@test.com");
            var result = service.ValidateToken(token);

            Assert.True(result.IsValid);
            Assert.NotNull(result.SecurityToken);
            Assert.Null(result.Exception);
        }

        [Fact]
        public void ValidateToken_ReturnsInvalid_ForInvalidToken()
        {
            var service = CreateService();
            // Use a properly formatted JWT with an invalid signature/content
            var invalidToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
            var result = service.ValidateToken(invalidToken);

            Assert.False(result.IsValid);
            Assert.NotNull(result.Exception);
        }

        #endregion

        #region RegisterNewUserAsync Tests

        [Fact]
        public async Task RegisterNewUserAsync_ReturnsError_WhenEmailFormatIsInvalid()
        {
            var emailHelper = new Mock<IEmailHelper>();
            emailHelper.Setup(e => e.VerifyEmail("invalidemail")).Returns(false);

            var service = CreateService(emailHelper: emailHelper);
            var errors = await service.RegisterNewUserAsync("John", "Doe", "invalidemail", "APP1");

            Assert.Single(errors);
            Assert.Contains("email format is invalid", errors);
        }

        [Fact]
        public async Task RegisterNewUserAsync_ReturnsError_WhenEmailAlreadyValidated()
        {
            var user = new User { Id = 1, Email = "test@test.com", IsEmailValidated = true, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync(user);

            var service = CreateService(userRepo);
            var errors = await service.RegisterNewUserAsync("John", "Doe", "test@test.com", "APP1");

            Assert.Single(errors);
            Assert.Contains("email is already used", errors);
        }

        [Fact]
        public async Task RegisterNewUserAsync_ReturnsError_WhenRegistrationOngoing()
        {
            var user = new User { Id = 1, Email = "test@test.com", IsEmailValidated = false, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync(user);

            var service = CreateService(userRepo);
            var errors = await service.RegisterNewUserAsync("John", "Doe", "test@test.com", "APP1");

            Assert.Single(errors);
            Assert.Contains("there's already a registration process ongoing, please check you mailbox to validate the email", errors);
        }

        [Fact]
        public async Task RegisterNewUserAsync_CreatesUser_WhenValid()
        {
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync((User?)null);
            userRepo.Setup(r => r.GetUsedEmailValidationHashesAsync()).ReturnsAsync(new List<string>());
            userRepo.Setup(r => r.CreateUserAsync(It.IsAny<User>())).ReturnsAsync((User u) => { u.Id = 1; return u; });

            var service = CreateService(userRepo);
            var errors = await service.RegisterNewUserAsync("John", "Doe", "test@test.com", null);

            Assert.Empty(errors);
            userRepo.Verify(r => r.CreateUserAsync(It.Is<User>(u => u.Email == "test@test.com" && u.FirstName == "John")), Times.Once);
        }

        [Fact]
        public async Task RegisterNewUserAsync_AssignsDefaultRole_WhenApplicationCodeProvided()
        {
            var user = new User { Id = 1, Email = "test@test.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var app = new Application { Id = 1, Code = "APP1", Name = "App1" };
            var role = new Role { Id = 1, Code = "USER", Name = "User", ApplicationId = 1, IsDefault = true };

            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync((User?)null);
            userRepo.Setup(r => r.GetUsedEmailValidationHashesAsync()).ReturnsAsync(new List<string>());
            userRepo.Setup(r => r.CreateUserAsync(It.IsAny<User>())).ReturnsAsync(user);

            var appRepo = new Mock<IApplicationRepository>();
            appRepo.Setup(r => r.GetApplicationByCodeAsync("APP1")).ReturnsAsync(app);

            var roleRepo = new Mock<IRoleRepository>();
            roleRepo.Setup(r => r.GetDefaultRoleByApplicationIdAsync(1)).ReturnsAsync(role);
            roleRepo.Setup(r => r.AssignRoleToUserAsync(1, 1, null)).ReturnsAsync(true);

            var service = CreateService(userRepo, appRepo: appRepo, roleRepo: roleRepo);
            var errors = await service.RegisterNewUserAsync("John", "Doe", "test@test.com", "APP1");

            Assert.Empty(errors);
            roleRepo.Verify(r => r.AssignRoleToUserAsync(1, 1, null), Times.Once);
        }

        [Fact]
        public async Task RegisterNewUserAsync_DeletesUser_WhenEmailSendFails()
        {
            var user = new User { Id = 1, Email = "test@test.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync((User?)null);
            userRepo.Setup(r => r.GetUsedEmailValidationHashesAsync()).ReturnsAsync(new List<string>());
            userRepo.Setup(r => r.CreateUserAsync(It.IsAny<User>())).ReturnsAsync(user);
            userRepo.Setup(r => r.DeleteUserAsync(user)).ReturnsAsync(true);

            var emailHelper = new Mock<IEmailHelper>();
            emailHelper.Setup(e => e.VerifyEmail(It.IsAny<string>())).Returns(true);
            emailHelper.Setup(e => e.GetEmailTemplate(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).Returns("<html></html>");
            emailHelper.Setup(e => e.SendEmail(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<ICollection<string>>()))
                .Throws(new Exception("Email send failed"));

            var service = CreateService(userRepo, emailHelper: emailHelper);

            await Assert.ThrowsAsync<Exception>(async () =>
                await service.RegisterNewUserAsync("John", "Doe", "test@test.com", null));

            userRepo.Verify(r => r.DeleteUserAsync(user), Times.Once);
        }

        #endregion

        #region ValidateEmailAsync Tests

        [Fact]
        public async Task ValidateEmailAsync_ReturnsFalse_WhenEmailFormatInvalid()
        {
            var emailHelper = new Mock<IEmailHelper>();
            emailHelper.Setup(e => e.VerifyEmail("invalidemail")).Returns(false);

            var service = CreateService(emailHelper: emailHelper);
            var result = await service.ValidateEmailAsync("hash", "invalidemail");

            Assert.False(result);
        }

        [Fact]
        public async Task ValidateEmailAsync_ReturnsFalse_WhenHashIsNotGuid()
        {
            var service = CreateService();
            var result = await service.ValidateEmailAsync("not-a-guid", "test@test.com");

            Assert.False(result);
        }

        [Fact]
        public async Task ValidateEmailAsync_ReturnsTrue_WhenValid()
        {
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.ValidateEmail(It.IsAny<string>(), "test@test.com")).ReturnsAsync(true);

            var service = CreateService(userRepo);
            var result = await service.ValidateEmailAsync(Guid.NewGuid().ToString(), "test@test.com");

            Assert.True(result);
        }

        #endregion

        #region ChangePasswordAsync Tests

        [Fact]
        public async Task ChangePasswordAsync_ReturnsFalse_WhenUserNotFound()
        {
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

            var service = CreateService(userRepo);
            var result = await service.ChangePasswordAsync("test@test.com", "old", "new");

            Assert.False(result);
        }

        [Fact]
        public async Task ChangePasswordAsync_ReturnsFalse_WhenAuthenticationNotFound()
        {
            var user = new User { Id = 1, Email = "test@test.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync(user);

            var authRepo = new Mock<IAuthenticationRepository>();
            authRepo.Setup(r => r.GetAuthenticationByUserIdAsync(1)).ReturnsAsync((Authentication?)null);

            var service = CreateService(userRepo, authRepo);
            var result = await service.ChangePasswordAsync("test@test.com", "old", "new");

            Assert.False(result);
        }

        [Fact]
        public async Task ChangePasswordAsync_ReturnsFalse_WhenOldPasswordIncorrect()
        {
            var user = new User { Id = 1, Email = "test@test.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var auth = new Authentication { UserId = 1, HashPassword = "hash", HashSalt = "salt" };

            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync(user);

            var authRepo = new Mock<IAuthenticationRepository>();
            authRepo.Setup(r => r.GetAuthenticationByUserIdAsync(1)).ReturnsAsync(auth);

            var crypto = new Mock<ICryptographyHelper>();
            crypto.Setup(c => c.VerifyPasswordHash("wrongold", It.IsAny<byte[]>(), It.IsAny<byte[]>())).Returns(false);

            var service = CreateService(userRepo, authRepo, crypto: crypto);
            var result = await service.ChangePasswordAsync("test@test.com", "wrongold", "new");

            Assert.False(result);
        }

        [Fact]
        public async Task ChangePasswordAsync_ReturnsTrue_WhenPasswordChanged()
        {
            var user = new User { Id = 1, Email = "test@test.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var auth = new Authentication { UserId = 1, HashPassword = "hash", HashSalt = "salt" };

            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync(user);

            var authRepo = new Mock<IAuthenticationRepository>();
            authRepo.Setup(r => r.GetAuthenticationByUserIdAsync(1)).ReturnsAsync(auth);
            authRepo.Setup(r => r.UpdateAuthenticationAsync(auth, false)).ReturnsAsync(true);

            var crypto = new Mock<ICryptographyHelper>();
            crypto.Setup(c => c.VerifyPasswordHash("old", It.IsAny<byte[]>(), It.IsAny<byte[]>())).Returns(true);
            crypto.Setup(c => c.GenerateRandomSalt()).Returns(Encoding.UTF8.GetBytes("newsalt"));
            crypto.Setup(c => c.GeneratePasswordHash("new", It.IsAny<byte[]>())).Returns(Encoding.UTF8.GetBytes("newhash"));

            var service = CreateService(userRepo, authRepo, crypto: crypto);
            var result = await service.ChangePasswordAsync("test@test.com", "old", "new");

            Assert.True(result);
            authRepo.Verify(r => r.UpdateAuthenticationAsync(auth, false), Times.Once);
        }

        #endregion

        #region ResetPasswordAsync Tests

        [Fact]
        public async Task ResetPasswordAsync_ReturnsFalse_WhenValidationFails()
        {
            var emailHelper = new Mock<IEmailHelper>();
            emailHelper.Setup(e => e.VerifyEmail(It.IsAny<string>())).Returns(false);

            var service = CreateService(emailHelper: emailHelper);
            var result = await service.ResetPasswordAsync("test@test.com", "hash", "newpass");

            Assert.False(result);
        }

        [Fact]
        public async Task ResetPasswordAsync_ReturnsFalse_WhenUserNotFound()
        {
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync((User?)null);
            userRepo.Setup(r => r.ValidateEmail(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            var service = CreateService(userRepo);
            var result = await service.ResetPasswordAsync("test@test.com", Guid.NewGuid().ToString(), "newpass");

            Assert.False(result);
        }

        [Fact]
        public async Task ResetPasswordAsync_CreatesAuthentication_WhenAuthNotExists()
        {
            var user = new User { Id = 1, Email = "test@test.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync(user);
            userRepo.Setup(r => r.ValidateEmail(It.IsAny<string>(), "test@test.com")).ReturnsAsync(true);

            var authRepo = new Mock<IAuthenticationRepository>();
            authRepo.Setup(r => r.GetAuthenticationByUserIdAsync(1)).ReturnsAsync((Authentication?)null);
            authRepo.Setup(r => r.CreateAuthenticationAsync(It.IsAny<Authentication>(), true)).ReturnsAsync(true);

            var crypto = new Mock<ICryptographyHelper>();
            crypto.Setup(c => c.GenerateRandomSalt()).Returns(Encoding.UTF8.GetBytes("salt"));
            crypto.Setup(c => c.GeneratePasswordHash("newpass", It.IsAny<byte[]>())).Returns(Encoding.UTF8.GetBytes("hash"));

            var service = CreateService(userRepo, authRepo, crypto: crypto);
            var result = await service.ResetPasswordAsync("test@test.com", Guid.NewGuid().ToString(), "newpass");

            Assert.True(result);
            authRepo.Verify(r => r.CreateAuthenticationAsync(It.Is<Authentication>(a => a.User == user && !a.IsTemporaryPassword), true), Times.Once);
        }

        [Fact]
        public async Task ResetPasswordAsync_UpdatesAuthentication_WhenAuthExists()
        {
            var user = new User { Id = 1, Email = "test@test.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var auth = new Authentication { UserId = 1, HashPassword = "oldhash", HashSalt = "oldsalt", IsTemporaryPassword = true };

            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync(user);
            userRepo.Setup(r => r.ValidateEmail(It.IsAny<string>(), "test@test.com")).ReturnsAsync(true);

            var authRepo = new Mock<IAuthenticationRepository>();
            authRepo.Setup(r => r.GetAuthenticationByUserIdAsync(1)).ReturnsAsync(auth);
            authRepo.Setup(r => r.UpdateAuthenticationAsync(auth, true)).ReturnsAsync(true);

            var crypto = new Mock<ICryptographyHelper>();
            crypto.Setup(c => c.GenerateRandomSalt()).Returns(Encoding.UTF8.GetBytes("newsalt"));
            crypto.Setup(c => c.GeneratePasswordHash("newpass", It.IsAny<byte[]>())).Returns(Encoding.UTF8.GetBytes("newhash"));

            var service = CreateService(userRepo, authRepo, crypto: crypto);
            var result = await service.ResetPasswordAsync("test@test.com", Guid.NewGuid().ToString(), "newpass");

            Assert.True(result);
            Assert.False(auth.IsTemporaryPassword);
            authRepo.Verify(r => r.UpdateAuthenticationAsync(auth, true), Times.Once);
        }

        #endregion

        #region RequestPasswordResetAsync Tests

        [Fact]
        public async Task RequestPasswordResetAsync_ReturnsFalse_WhenUserNotFound()
        {
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

            var service = CreateService(userRepo);
            var result = await service.RequestPasswordResetAsync("test@test.com");

            Assert.False(result);
        }

        [Fact]
        public async Task RequestPasswordResetAsync_ReturnsFalse_WhenEmailNotValidated()
        {
            var user = new User { Id = 1, Email = "test@test.com", IsEmailValidated = false, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync(user);

            var service = CreateService(userRepo);
            var result = await service.RequestPasswordResetAsync("test@test.com");

            Assert.False(result);
        }

        [Fact]
        public async Task RequestPasswordResetAsync_ReturnsFalse_WhenAuthenticationNotFound()
        {
            var user = new User { Id = 1, Email = "test@test.com", IsEmailValidated = true, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync(user);

            var authRepo = new Mock<IAuthenticationRepository>();
            authRepo.Setup(r => r.GetAuthenticationByUserIdAsync(1)).ReturnsAsync((Authentication?)null);

            var service = CreateService(userRepo, authRepo);
            var result = await service.RequestPasswordResetAsync("test@test.com");

            Assert.False(result);
        }

        [Fact]
        public async Task RequestPasswordResetAsync_ReturnsFalse_WhenUpdateFails()
        {
            var user = new User { Id = 1, Email = "test@test.com", IsEmailValidated = true, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var auth = new Authentication { UserId = 1, HashPassword = "hash", HashSalt = "salt" };

            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync(user);

            var authRepo = new Mock<IAuthenticationRepository>();
            authRepo.Setup(r => r.GetAuthenticationByUserIdAsync(1)).ReturnsAsync(auth);
            authRepo.Setup(r => r.UpdateAuthenticationAsync(auth, false)).ReturnsAsync(false);

            var service = CreateService(userRepo, authRepo);
            var result = await service.RequestPasswordResetAsync("test@test.com");

            Assert.False(result);
        }

        [Fact]
        public async Task RequestPasswordResetAsync_ReturnsTrue_WhenSuccessful()
        {
            var user = new User { Id = 1, Email = "test@test.com", IsEmailValidated = true, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var auth = new Authentication { UserId = 1, HashPassword = "hash", HashSalt = "salt" };

            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync(user);

            var authRepo = new Mock<IAuthenticationRepository>();
            authRepo.Setup(r => r.GetAuthenticationByUserIdAsync(1)).ReturnsAsync(auth);
            authRepo.Setup(r => r.UpdateAuthenticationAsync(auth, false)).ReturnsAsync(true);

            var service = CreateService(userRepo, authRepo);
            var result = await service.RequestPasswordResetAsync("test@test.com");

            Assert.True(result);
            Assert.NotNull(auth.PasswordResetHash);
            Assert.NotNull(auth.PasswordResetRequestedAt);
        }

        #endregion
    }
}
