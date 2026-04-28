using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Services;
using Touir.ExpensesManager.Users.Repositories.Contracts;
using Touir.ExpensesManager.Users.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Moq;

namespace Touir.ExpensesManager.Users.Tests.Services
{
    public class RefreshTokenServiceTests
    {
        private static RefreshTokenService CreateService(Mock<IRefreshTokenRepository>? repo = null, int refreshExpiryInDays = 7)
        {
            var options = Options.Create(new JwtAuthOptions
            {
                SecretKey = "supersecretkey12345678901234567890",
                Issuer = "issuer",
                Audience = "audience",
                ExpiryInMinutes = 60,
                RefreshExpiryInDays = refreshExpiryInDays
            });
            return new RefreshTokenService(repo?.Object ?? new Mock<IRefreshTokenRepository>().Object, options);
        }

        #region GenerateAsync Tests

        [Fact]
        public async Task GenerateAsync_ReturnsNonEmptyToken()
        {
            var repo = new Mock<IRefreshTokenRepository>();
            repo.Setup(r => r.AddAsync(It.IsAny<RefreshToken>())).Returns(Task.CompletedTask);

            var service = CreateService(repo);
            var token = await service.GenerateAsync(1, false);

            Assert.False(string.IsNullOrWhiteSpace(token));
        }

        [Fact]
        public async Task GenerateAsync_PersistsRefreshToken()
        {
            var repo = new Mock<IRefreshTokenRepository>();
            repo.Setup(r => r.AddAsync(It.IsAny<RefreshToken>())).Returns(Task.CompletedTask);

            var service = CreateService(repo);
            var token = await service.GenerateAsync(1, false);

            repo.Verify(r => r.AddAsync(It.Is<RefreshToken>(t =>
                t.Token == token &&
                t.UserId == 1)), Times.Once);
        }

        [Fact]
        public async Task GenerateAsync_ExpiresInOneDay_WhenRememberMeFalse()
        {
            RefreshToken? saved = null;
            var repo = new Mock<IRefreshTokenRepository>();
            repo.Setup(r => r.AddAsync(It.IsAny<RefreshToken>()))
                .Callback<RefreshToken>(t => saved = t)
                .Returns(Task.CompletedTask);

            var service = CreateService(repo, refreshExpiryInDays: 30);
            var before = DateTime.UtcNow;
            await service.GenerateAsync(1, false);
            var after = DateTime.UtcNow;

            Assert.NotNull(saved);
            Assert.True(saved!.ExpiresAt >= before.AddDays(1));
            Assert.True(saved!.ExpiresAt <= after.AddDays(1));
        }

        [Fact]
        public async Task GenerateAsync_ExpiresInConfiguredDays_WhenRememberMeTrue()
        {
            RefreshToken? saved = null;
            var repo = new Mock<IRefreshTokenRepository>();
            repo.Setup(r => r.AddAsync(It.IsAny<RefreshToken>()))
                .Callback<RefreshToken>(t => saved = t)
                .Returns(Task.CompletedTask);

            var service = CreateService(repo, refreshExpiryInDays: 30);
            var before = DateTime.UtcNow;
            await service.GenerateAsync(1, true);
            var after = DateTime.UtcNow;

            Assert.NotNull(saved);
            Assert.True(saved!.ExpiresAt >= before.AddDays(30));
            Assert.True(saved!.ExpiresAt <= after.AddDays(30));
        }

        [Fact]
        public async Task GenerateAsync_GeneratesUniqueTokens()
        {
            var repo = new Mock<IRefreshTokenRepository>();
            repo.Setup(r => r.AddAsync(It.IsAny<RefreshToken>())).Returns(Task.CompletedTask);

            var service = CreateService(repo);
            var token1 = await service.GenerateAsync(1, false);
            var token2 = await service.GenerateAsync(1, false);

            Assert.NotEqual(token1, token2);
        }

        #endregion

        #region ValidateAsync Tests

        [Fact]
        public async Task ValidateAsync_ReturnsInvalid_WhenTokenNotFound()
        {
            var repo = new Mock<IRefreshTokenRepository>();
            repo.Setup(r => r.GetActiveByTokenAsync("missing")).ReturnsAsync((RefreshToken?)null);

            var service = CreateService(repo);
            var result = await service.ValidateAsync("missing");

            Assert.False(result.isValid);
            Assert.Equal(0, result.userId);
        }

        [Fact]
        public async Task ValidateAsync_ReturnsInvalid_WhenTokenIsInactive()
        {
            var token = new RefreshToken
            {
                Token = "tok",
                UserId = 5,
                ExpiresAt = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            };
            var repo = new Mock<IRefreshTokenRepository>();
            repo.Setup(r => r.GetActiveByTokenAsync("tok")).ReturnsAsync(token);

            var service = CreateService(repo);
            var result = await service.ValidateAsync("tok");

            Assert.False(result.isValid);
            Assert.Equal(0, result.userId);
        }

        [Fact]
        public async Task ValidateAsync_ReturnsValidWithUserId_WhenTokenIsActive()
        {
            var token = new RefreshToken
            {
                Token = "tok",
                UserId = 5,
                ExpiresAt = DateTime.UtcNow.AddDays(1),
                CreatedAt = DateTime.UtcNow
            };
            var repo = new Mock<IRefreshTokenRepository>();
            repo.Setup(r => r.GetActiveByTokenAsync("tok")).ReturnsAsync(token);

            var service = CreateService(repo);
            var result = await service.ValidateAsync("tok");

            Assert.True(result.isValid);
            Assert.Equal(5, result.userId);
        }

        #endregion

        #region RevokeAsync Tests

        [Fact]
        public async Task RevokeAsync_DoesNothing_WhenTokenNotFound()
        {
            var repo = new Mock<IRefreshTokenRepository>();
            repo.Setup(r => r.GetActiveByTokenAsync("missing")).ReturnsAsync((RefreshToken?)null);

            var service = CreateService(repo);
            await service.RevokeAsync("missing");

            repo.Verify(r => r.RevokeAsync(It.IsAny<RefreshToken>()), Times.Never);
        }

        [Fact]
        public async Task RevokeAsync_RevokesToken_WhenFound()
        {
            var token = new RefreshToken
            {
                Token = "tok",
                UserId = 1,
                ExpiresAt = DateTime.UtcNow.AddDays(1),
                CreatedAt = DateTime.UtcNow
            };
            var repo = new Mock<IRefreshTokenRepository>();
            repo.Setup(r => r.GetActiveByTokenAsync("tok")).ReturnsAsync(token);
            repo.Setup(r => r.RevokeAsync(token)).Returns(Task.CompletedTask);

            var service = CreateService(repo);
            await service.RevokeAsync("tok");

            repo.Verify(r => r.RevokeAsync(token), Times.Once);
        }

        #endregion

        #region RevokeAllForUserAsync Tests

        [Fact]
        public async Task RevokeAllForUserAsync_DelegatesToRepository()
        {
            var repo = new Mock<IRefreshTokenRepository>();
            repo.Setup(r => r.RevokeAllByUserIdAsync(42)).Returns(Task.CompletedTask);

            var service = CreateService(repo);
            await service.RevokeAllForUserAsync(42);

            repo.Verify(r => r.RevokeAllByUserIdAsync(42), Times.Once);
        }

        #endregion
    }
}
