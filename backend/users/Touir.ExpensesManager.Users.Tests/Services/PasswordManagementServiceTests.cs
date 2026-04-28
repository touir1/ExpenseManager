using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Services;
using Touir.ExpensesManager.Users.Repositories.Contracts;
using Touir.ExpensesManager.Users.Infrastructure.Contracts;
using Touir.ExpensesManager.Users.Infrastructure.Options;
using Touir.ExpensesManager.Users.Infrastructure;
using Microsoft.Extensions.Options;
using Moq;
using System.Text;

namespace Touir.ExpensesManager.Users.Tests.Services
{
    public class PasswordManagementServiceTests
    {
        private static Mock<IEmailHelper> CreateEmailHelperMock()
        {
            var mock = new Mock<IEmailHelper>();
            mock.Setup(e => e.VerifyEmail(It.IsAny<string>())).Returns(true);
            mock.Setup(e => e.GetEmailTemplate(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).Returns("<html></html>");
            mock.Setup(e => e.SendEmail(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<ICollection<string>>())).Returns(true);
            return mock;
        }

        private static PasswordManagementService CreateService(
            Mock<IUserRepository>? userRepo = null,
            Mock<IAuthenticationRepository>? authRepo = null,
            Mock<ICryptographyHelper>? crypto = null,
            Mock<IEmailHelper>? emailHelper = null)
        {
            return new PasswordManagementService(
                Options.Create(new AuthenticationServiceOptions { VerifyEmailBaseUrl = "http://localhost/verify" }),
                emailHelper?.Object ?? CreateEmailHelperMock().Object,
                crypto?.Object ?? new Mock<ICryptographyHelper>().Object,
                userRepo?.Object ?? new Mock<IUserRepository>().Object,
                authRepo?.Object ?? new Mock<IAuthenticationRepository>().Object
            );
        }

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

            var service = CreateService(userRepo, authRepo, crypto);
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

            var service = CreateService(userRepo, authRepo, crypto);
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

            var service = CreateService(userRepo, authRepo, crypto);
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

            var service = CreateService(userRepo, authRepo, crypto);
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
            var result = await service.RequestPasswordResetAsync("test@test.com", "EXPENSES_MANAGER");

            Assert.False(result);
        }

        [Fact]
        public async Task RequestPasswordResetAsync_ReturnsFalse_WhenEmailNotValidated()
        {
            var user = new User { Id = 1, Email = "test@test.com", IsEmailValidated = false, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync(user);

            var service = CreateService(userRepo);
            var result = await service.RequestPasswordResetAsync("test@test.com", "EXPENSES_MANAGER");

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
            var result = await service.RequestPasswordResetAsync("test@test.com", "EXPENSES_MANAGER");

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
            var result = await service.RequestPasswordResetAsync("test@test.com", "EXPENSES_MANAGER");

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
            var result = await service.RequestPasswordResetAsync("test@test.com", "EXPENSES_MANAGER");

            Assert.True(result);
            Assert.NotNull(auth.PasswordResetHash);
            Assert.NotNull(auth.PasswordResetRequestedAt);
        }

        [Fact]
        public async Task RequestPasswordResetAsync_SendsEmailWithPasswordResetTemplate()
        {
            var user = new User { Id = 1, Email = "test@test.com", IsEmailValidated = true, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var auth = new Authentication { UserId = 1, HashPassword = "hash", HashSalt = "salt" };

            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync(user);

            var authRepo = new Mock<IAuthenticationRepository>();
            authRepo.Setup(r => r.GetAuthenticationByUserIdAsync(1)).ReturnsAsync(auth);
            authRepo.Setup(r => r.UpdateAuthenticationAsync(auth, false)).ReturnsAsync(true);

            var emailHelper = new Mock<IEmailHelper>();
            emailHelper.Setup(e => e.VerifyEmail(It.IsAny<string>())).Returns(true);
            emailHelper.Setup(e => e.GetEmailTemplate(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).Returns("<html></html>");
            emailHelper.Setup(e => e.SendEmail(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<ICollection<string>>())).Returns(true);

            var service = CreateService(userRepo, authRepo, emailHelper: emailHelper);
            await service.RequestPasswordResetAsync("test@test.com", "EXPENSES_MANAGER");

            emailHelper.Verify(e => e.GetEmailTemplate(
                EmailHtmlTemplate.PasswordReset.Key,
                It.Is<Dictionary<string, string>>(d =>
                    d.ContainsKey(EmailHtmlTemplate.PasswordReset.Variables.ResetLink))),
                Times.Once);
            emailHelper.Verify(e => e.SendEmail(
                "test@test.com",
                It.IsAny<string>(), It.IsAny<string>(),
                "[Expenses Manager] Password Reset",
                It.IsAny<string>(), true,
                It.IsAny<ICollection<string>>()),
                Times.Once);
        }

        [Fact]
        public async Task RequestPasswordResetAsync_ReturnsFalse_WhenEmailSendThrows()
        {
            var user = new User { Id = 1, Email = "test@test.com", IsEmailValidated = true, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var auth = new Authentication { UserId = 1, HashPassword = "hash", HashSalt = "salt" };

            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync(user);

            var authRepo = new Mock<IAuthenticationRepository>();
            authRepo.Setup(r => r.GetAuthenticationByUserIdAsync(1)).ReturnsAsync(auth);
            authRepo.Setup(r => r.UpdateAuthenticationAsync(auth, false)).ReturnsAsync(true);

            var emailHelper = new Mock<IEmailHelper>();
            emailHelper.Setup(e => e.VerifyEmail(It.IsAny<string>())).Returns(true);
            emailHelper.Setup(e => e.GetEmailTemplate(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .Throws(new Exception("Email template failed"));

            var service = CreateService(userRepo, authRepo, emailHelper: emailHelper);
            var result = await service.RequestPasswordResetAsync("test@test.com", "EXPENSES_MANAGER");

            Assert.False(result);
        }

        #endregion
    }
}
