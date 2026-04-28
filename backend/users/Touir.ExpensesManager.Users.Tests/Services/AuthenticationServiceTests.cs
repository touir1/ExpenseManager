using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Services;
using Touir.ExpensesManager.Users.Repositories.Contracts;
using Touir.ExpensesManager.Users.Infrastructure.Contracts;
using Moq;

namespace Touir.ExpensesManager.Users.Tests.Services
{
    public class AuthenticationServiceTests
    {
        private static AuthenticationService CreateService(
            Mock<IUserRepository>? userRepo = null,
            Mock<IAuthenticationRepository>? authRepo = null,
            Mock<ICryptographyHelper>? crypto = null)
        {
            return new AuthenticationService(
                crypto?.Object ?? new Mock<ICryptographyHelper>().Object,
                userRepo?.Object ?? new Mock<IUserRepository>().Object,
                authRepo?.Object ?? new Mock<IAuthenticationRepository>().Object
            );
        }

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

            var service = CreateService(userRepo, authRepo, crypto);
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

            var service = CreateService(userRepo, authRepo, crypto);
            var result = await service.AuthenticateAsync("test@test.com", "wrongpassword");

            Assert.Null(result);
        }
    }
}
