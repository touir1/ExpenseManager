using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Services;
using Touir.ExpensesManager.Users.Repositories.Contracts;
using Touir.ExpensesManager.Users.Infrastructure.Contracts;
using Touir.ExpensesManager.Users.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Moq;
using System.Text;

namespace Touir.ExpensesManager.Users.Tests.Services
{
    public class PasswordManagementServiceTests
    {
        private static Mock<IOutboxRepository> CreateDefaultOutboxMock()
        {
            var mock = new Mock<IOutboxRepository>();
            mock.Setup(r => r.EnqueueAsync(It.IsAny<OutboxEvent>())).Returns(Task.CompletedTask);
            return mock;
        }

        private static PasswordManagementService CreateService(
            Mock<IUserRepository>? userRepo = null,
            Mock<IAuthenticationRepository>? authRepo = null,
            Mock<ICryptographyHelper>? crypto = null,
            Mock<IOutboxRepository>? outboxRepo = null)
        {
            var outbox = outboxRepo ?? CreateDefaultOutboxMock();

            return new PasswordManagementService(
                Options.Create(new AuthenticationServiceOptions
                {
                    VerifyEmailBaseUrl = "http://localhost/verify",
                    ResetPasswordBaseUrl = "http://localhost/reset-password",
                    PasswordResetExpiryInHours = 24
                }),
                crypto?.Object ?? new Mock<ICryptographyHelper>().Object,
                userRepo?.Object ?? new Mock<IUserRepository>().Object,
                authRepo?.Object ?? new Mock<IAuthenticationRepository>().Object,
                outbox.Object
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

        [Fact]
        public async Task ChangePasswordAsync_EnqueuesPasswordChangedEvent_WhenSuccessful()
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

            var outboxRepo = new Mock<IOutboxRepository>();
            outboxRepo.Setup(r => r.EnqueueAsync(It.IsAny<OutboxEvent>())).Returns(Task.CompletedTask);

            var service = CreateService(userRepo, authRepo, crypto, outboxRepo);
            await service.ChangePasswordAsync("test@test.com", "old", "new");

            outboxRepo.Verify(r => r.EnqueueAsync(It.Is<OutboxEvent>(e =>
                e.EventType == "user.password.changed")), Times.Once);
        }

        #endregion

        #region CreatePasswordAsync Tests

        [Fact]
        public async Task CreatePasswordAsync_ReturnsFalse_WhenEmailInvalid()
        {
            var service = CreateService();
            var result = await service.CreatePasswordAsync("invalidemail", Guid.NewGuid().ToString(), "newpass");

            Assert.False(result);
        }

        [Fact]
        public async Task CreatePasswordAsync_ReturnsFalse_WhenVerificationHashNotGuid()
        {
            var service = CreateService();
            var result = await service.CreatePasswordAsync("test@test.com", "not-a-guid", "newpass");
            Assert.False(result);
        }

        [Fact]
        public async Task CreatePasswordAsync_ReturnsFalse_WhenUserNotFound()
        {
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync((User?)null);

            var service = CreateService(userRepo);
            var result = await service.CreatePasswordAsync("test@test.com", Guid.NewGuid().ToString(), "newpass");

            Assert.False(result);
        }

        [Fact]
        public async Task CreatePasswordAsync_ReturnsFalse_WhenEmailValidationFails()
        {
            var user = new User { Id = 1, Email = "test@test.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync(user);
            userRepo.Setup(r => r.ValidateEmail(It.IsAny<string>(), "test@test.com")).ReturnsAsync(false);

            var service = CreateService(userRepo);
            var result = await service.CreatePasswordAsync("test@test.com", Guid.NewGuid().ToString(), "newpass");

            Assert.False(result);
        }

        [Fact]
        public async Task CreatePasswordAsync_CreatesAuthentication_WhenAuthNotExists()
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
            var result = await service.CreatePasswordAsync("test@test.com", Guid.NewGuid().ToString(), "newpass");

            Assert.True(result);
            authRepo.Verify(r => r.CreateAuthenticationAsync(It.Is<Authentication>(a => a.User == user && !a.IsTemporaryPassword), true), Times.Once);
        }

        [Fact]
        public async Task CreatePasswordAsync_UpdatesAuthentication_WhenAuthExists()
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
            var result = await service.CreatePasswordAsync("test@test.com", Guid.NewGuid().ToString(), "newpass");

            Assert.True(result);
            Assert.False(auth.IsTemporaryPassword);
            authRepo.Verify(r => r.UpdateAuthenticationAsync(auth, true), Times.Once);
        }

        #endregion

        #region ResetPasswordAsync Tests

        [Fact]
        public async Task ResetPasswordAsync_ReturnsFalse_WhenEmailInvalid()
        {
            var service = CreateService();
            var result = await service.ResetPasswordAsync("invalidemail", Guid.NewGuid().ToString(), "newpass");

            Assert.False(result);
        }

        [Fact]
        public async Task ResetPasswordAsync_ReturnsFalse_WhenResetHashNotGuid()
        {
            var service = CreateService();
            var result = await service.ResetPasswordAsync("test@test.com", "not-a-guid", "newpass");
            Assert.False(result);
        }

        [Fact]
        public async Task ResetPasswordAsync_ReturnsFalse_WhenUserNotFound()
        {
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync((User?)null);

            var service = CreateService(userRepo);
            var result = await service.ResetPasswordAsync("test@test.com", Guid.NewGuid().ToString(), "newpass");

            Assert.False(result);
        }

        [Fact]
        public async Task ResetPasswordAsync_ReturnsFalse_WhenAuthNotFound()
        {
            var user = new User { Id = 1, Email = "test@test.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync(user);

            var authRepo = new Mock<IAuthenticationRepository>();
            authRepo.Setup(r => r.GetAuthenticationByUserIdAsync(1)).ReturnsAsync((Authentication?)null);

            var service = CreateService(userRepo, authRepo);
            var result = await service.ResetPasswordAsync("test@test.com", Guid.NewGuid().ToString(), "newpass");

            Assert.False(result);
        }

        [Fact]
        public async Task ResetPasswordAsync_ReturnsFalse_WhenHashMismatch()
        {
            var user = new User { Id = 1, Email = "test@test.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var auth = new Authentication
            {
                UserId = 1,
                HashPassword = "hash",
                HashSalt = "salt",
                PasswordResetHash = Guid.NewGuid().ToString(),
                PasswordResetRequestedAt = DateTime.UtcNow.AddMinutes(-10)
            };

            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync(user);

            var authRepo = new Mock<IAuthenticationRepository>();
            authRepo.Setup(r => r.GetAuthenticationByUserIdAsync(1)).ReturnsAsync(auth);

            var service = CreateService(userRepo, authRepo);
            var result = await service.ResetPasswordAsync("test@test.com", Guid.NewGuid().ToString(), "newpass");

            Assert.False(result);
        }

        [Fact]
        public async Task ResetPasswordAsync_ReturnsFalse_WhenHashExpired()
        {
            var resetHash = Guid.NewGuid().ToString();
            var user = new User { Id = 1, Email = "test@test.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var auth = new Authentication
            {
                UserId = 1,
                HashPassword = "oldhash",
                HashSalt = "oldsalt",
                PasswordResetHash = resetHash,
                PasswordResetRequestedAt = DateTime.UtcNow.AddHours(-25)
            };

            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync(user);

            var authRepo = new Mock<IAuthenticationRepository>();
            authRepo.Setup(r => r.GetAuthenticationByUserIdAsync(1)).ReturnsAsync(auth);

            var service = CreateService(userRepo, authRepo);
            var result = await service.ResetPasswordAsync("test@test.com", resetHash, "newpass");

            Assert.False(result);
        }

        [Fact]
        public async Task ResetPasswordAsync_ResetsPassword_WhenHashValid()
        {
            var resetHash = Guid.NewGuid().ToString();
            var user = new User { Id = 1, Email = "test@test.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var auth = new Authentication
            {
                UserId = 1,
                HashPassword = "oldhash",
                HashSalt = "oldsalt",
                IsTemporaryPassword = false,
                PasswordResetHash = resetHash,
                PasswordResetRequestedAt = DateTime.UtcNow.AddMinutes(-10)
            };

            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync(user);

            var authRepo = new Mock<IAuthenticationRepository>();
            authRepo.Setup(r => r.GetAuthenticationByUserIdAsync(1)).ReturnsAsync(auth);
            authRepo.Setup(r => r.UpdateAuthenticationAsync(auth, false)).ReturnsAsync(true);

            var crypto = new Mock<ICryptographyHelper>();
            crypto.Setup(c => c.GenerateRandomSalt()).Returns(Encoding.UTF8.GetBytes("newsalt"));
            crypto.Setup(c => c.GeneratePasswordHash("newpass", It.IsAny<byte[]>())).Returns(Encoding.UTF8.GetBytes("newhash"));

            var service = CreateService(userRepo, authRepo, crypto);
            var result = await service.ResetPasswordAsync("test@test.com", resetHash, "newpass");

            Assert.True(result);
            Assert.Null(auth.PasswordResetHash);
            Assert.Null(auth.PasswordResetRequestedAt);
            authRepo.Verify(r => r.UpdateAuthenticationAsync(auth, false), Times.Once);
        }

        [Fact]
        public async Task ResetPasswordAsync_EnqueuesPasswordChangedEvent_WhenSuccessful()
        {
            var resetHash = Guid.NewGuid().ToString();
            var user = new User { Id = 1, Email = "test@test.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var auth = new Authentication
            {
                UserId = 1,
                HashPassword = "oldhash",
                HashSalt = "oldsalt",
                PasswordResetHash = resetHash,
                PasswordResetRequestedAt = DateTime.UtcNow.AddMinutes(-10)
            };

            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync(user);

            var authRepo = new Mock<IAuthenticationRepository>();
            authRepo.Setup(r => r.GetAuthenticationByUserIdAsync(1)).ReturnsAsync(auth);
            authRepo.Setup(r => r.UpdateAuthenticationAsync(auth, false)).ReturnsAsync(true);

            var crypto = new Mock<ICryptographyHelper>();
            crypto.Setup(c => c.GenerateRandomSalt()).Returns(Encoding.UTF8.GetBytes("newsalt"));
            crypto.Setup(c => c.GeneratePasswordHash("newpass", It.IsAny<byte[]>())).Returns(Encoding.UTF8.GetBytes("newhash"));

            var outboxRepo = new Mock<IOutboxRepository>();
            outboxRepo.Setup(r => r.EnqueueAsync(It.IsAny<OutboxEvent>())).Returns(Task.CompletedTask);

            var service = CreateService(userRepo, authRepo, crypto, outboxRepo);
            await service.ResetPasswordAsync("test@test.com", resetHash, "newpass");

            outboxRepo.Verify(r => r.EnqueueAsync(It.Is<OutboxEvent>(e =>
                e.EventType == "user.password.changed")), Times.Once);
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
        public async Task RequestPasswordResetAsync_EnqueuesOutboxEvent_WhenSuccessful()
        {
            var user = new User { Id = 1, Email = "test@test.com", IsEmailValidated = true, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var auth = new Authentication { UserId = 1, HashPassword = "hash", HashSalt = "salt" };

            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync(user);

            var authRepo = new Mock<IAuthenticationRepository>();
            authRepo.Setup(r => r.GetAuthenticationByUserIdAsync(1)).ReturnsAsync(auth);
            authRepo.Setup(r => r.UpdateAuthenticationAsync(auth, false)).ReturnsAsync(true);

            var outboxRepo = new Mock<IOutboxRepository>();
            outboxRepo.Setup(r => r.EnqueueAsync(It.IsAny<OutboxEvent>())).Returns(Task.CompletedTask);

            var service = CreateService(userRepo, authRepo, outboxRepo: outboxRepo);
            await service.RequestPasswordResetAsync("test@test.com", "EXPENSES_MANAGER");

            outboxRepo.Verify(r => r.EnqueueAsync(It.Is<OutboxEvent>(e =>
                e.EventType == "user.password.reset.requested")), Times.Once);
        }

        #endregion
    }
}
