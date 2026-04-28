using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Repositories;
using Touir.ExpensesManager.Users.Tests.Repositories.Helpers;

namespace Touir.ExpensesManager.Users.Tests.Repositories
{
    public class RefreshTokenRepositoryTests
    {
        private static User MakeUser(int id, string email) => new()
        {
            Id = id,
            FirstName = "F",
            LastName = "L",
            Email = email,
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow
        };

        #region GetActiveByTokenAsync Tests

        [Fact]
        public async Task GetActiveByTokenAsync_ReturnsToken_WhenActiveAndNotExpired()
        {
            using var db = new TestDbContextWrapper();
            var user = MakeUser(1, "a@b.com");
            db.Context.Users.Add(user);
            var token = new RefreshToken
            {
                Token = "active-token",
                UserId = 1,
                User = user,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };
            db.Context.RefreshTokens.Add(token);
            db.Context.SaveChanges();

            var repo = new RefreshTokenRepository(db.Context);
            var result = await repo.GetActiveByTokenAsync("active-token");

            Assert.NotNull(result);
            Assert.Equal("active-token", result.Token);
        }

        [Fact]
        public async Task GetActiveByTokenAsync_ReturnsNull_WhenTokenNotFound()
        {
            using var db = new TestDbContextWrapper();
            var repo = new RefreshTokenRepository(db.Context);

            var result = await repo.GetActiveByTokenAsync("nonexistent");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetActiveByTokenAsync_ReturnsNull_WhenTokenExpired()
        {
            using var db = new TestDbContextWrapper();
            var user = MakeUser(2, "b@c.com");
            db.Context.Users.Add(user);
            var token = new RefreshToken
            {
                Token = "expired-token",
                UserId = 2,
                User = user,
                ExpiresAt = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-8)
            };
            db.Context.RefreshTokens.Add(token);
            db.Context.SaveChanges();

            var repo = new RefreshTokenRepository(db.Context);
            var result = await repo.GetActiveByTokenAsync("expired-token");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetActiveByTokenAsync_ReturnsNull_WhenTokenRevoked()
        {
            using var db = new TestDbContextWrapper();
            var user = MakeUser(3, "c@d.com");
            db.Context.Users.Add(user);
            var token = new RefreshToken
            {
                Token = "revoked-token",
                UserId = 3,
                User = user,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                RevokedAt = DateTime.UtcNow.AddMinutes(-5)
            };
            db.Context.RefreshTokens.Add(token);
            db.Context.SaveChanges();

            var repo = new RefreshTokenRepository(db.Context);
            var result = await repo.GetActiveByTokenAsync("revoked-token");

            Assert.Null(result);
        }

        #endregion

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_PersistsRefreshToken()
        {
            using var db = new TestDbContextWrapper();
            var user = MakeUser(4, "d@e.com");
            db.Context.Users.Add(user);
            db.Context.SaveChanges();

            var repo = new RefreshTokenRepository(db.Context);
            var token = new RefreshToken
            {
                Token = "new-token",
                UserId = 4,
                User = user,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };

            await repo.AddAsync(token);

            var saved = db.Context.RefreshTokens.FirstOrDefault(t => t.Token == "new-token");
            Assert.NotNull(saved);
            Assert.Equal(4, saved.UserId);
        }

        #endregion

        #region RevokeAsync Tests

        [Fact]
        public async Task RevokeAsync_SetsRevokedAt()
        {
            using var db = new TestDbContextWrapper();
            var user = MakeUser(5, "e@f.com");
            db.Context.Users.Add(user);
            var token = new RefreshToken
            {
                Token = "to-revoke",
                UserId = 5,
                User = user,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };
            db.Context.RefreshTokens.Add(token);
            db.Context.SaveChanges();

            var repo = new RefreshTokenRepository(db.Context);
            var dbToken = db.Context.RefreshTokens.First(t => t.Token == "to-revoke");
            var before = DateTime.UtcNow;

            await repo.RevokeAsync(dbToken);

            var updated = db.Context.RefreshTokens.First(t => t.Token == "to-revoke");
            Assert.NotNull(updated.RevokedAt);
            Assert.True(updated.RevokedAt >= before);
        }

        [Fact]
        public async Task RevokeAsync_MakesTokenInactive()
        {
            using var db = new TestDbContextWrapper();
            var user = MakeUser(6, "f@g.com");
            db.Context.Users.Add(user);
            var token = new RefreshToken
            {
                Token = "to-revoke-2",
                UserId = 6,
                User = user,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };
            db.Context.RefreshTokens.Add(token);
            db.Context.SaveChanges();

            var repo = new RefreshTokenRepository(db.Context);
            var dbToken = db.Context.RefreshTokens.First(t => t.Token == "to-revoke-2");
            await repo.RevokeAsync(dbToken);

            Assert.False(dbToken.IsActive);
        }

        #endregion

        #region RevokeAllByUserIdAsync Tests

        [Fact]
        public async Task RevokeAllByUserIdAsync_RevokesAllActiveTokensForUser()
        {
            using var db = new TestDbContextWrapper();
            var user = MakeUser(7, "g@h.com");
            db.Context.Users.Add(user);
            db.Context.RefreshTokens.AddRange(
                new RefreshToken { Token = "tok-a", UserId = 7, User = user, ExpiresAt = DateTime.UtcNow.AddDays(7), CreatedAt = DateTime.UtcNow },
                new RefreshToken { Token = "tok-b", UserId = 7, User = user, ExpiresAt = DateTime.UtcNow.AddDays(3), CreatedAt = DateTime.UtcNow }
            );
            db.Context.SaveChanges();

            var repo = new RefreshTokenRepository(db.Context);
            await repo.RevokeAllByUserIdAsync(7);

            var remaining = db.Context.RefreshTokens.Where(t => t.UserId == 7).ToList();
            Assert.All(remaining, t => Assert.NotNull(t.RevokedAt));
        }

        [Fact]
        public async Task RevokeAllByUserIdAsync_DoesNotRevokeAlreadyRevokedTokens()
        {
            using var db = new TestDbContextWrapper();
            var user = MakeUser(8, "h@i.com");
            db.Context.Users.Add(user);
            var alreadyRevoked = DateTime.UtcNow.AddHours(-1);
            db.Context.RefreshTokens.AddRange(
                new RefreshToken { Token = "tok-c", UserId = 8, User = user, ExpiresAt = DateTime.UtcNow.AddDays(7), CreatedAt = DateTime.UtcNow },
                new RefreshToken { Token = "tok-d", UserId = 8, User = user, ExpiresAt = DateTime.UtcNow.AddDays(7), CreatedAt = DateTime.UtcNow, RevokedAt = alreadyRevoked }
            );
            db.Context.SaveChanges();

            var repo = new RefreshTokenRepository(db.Context);
            await repo.RevokeAllByUserIdAsync(8);

            var preserved = db.Context.RefreshTokens.First(t => t.Token == "tok-d");
            Assert.Equal(alreadyRevoked, preserved.RevokedAt);
        }

        [Fact]
        public async Task RevokeAllByUserIdAsync_DoesNotRevokeExpiredTokens()
        {
            using var db = new TestDbContextWrapper();
            var user = MakeUser(9, "i@j.com");
            db.Context.Users.Add(user);
            db.Context.RefreshTokens.AddRange(
                new RefreshToken { Token = "tok-e", UserId = 9, User = user, ExpiresAt = DateTime.UtcNow.AddDays(7), CreatedAt = DateTime.UtcNow },
                new RefreshToken { Token = "tok-f", UserId = 9, User = user, ExpiresAt = DateTime.UtcNow.AddDays(-1), CreatedAt = DateTime.UtcNow.AddDays(-8) }
            );
            db.Context.SaveChanges();

            var repo = new RefreshTokenRepository(db.Context);
            await repo.RevokeAllByUserIdAsync(9);

            var expired = db.Context.RefreshTokens.First(t => t.Token == "tok-f");
            Assert.Null(expired.RevokedAt);
        }

        [Fact]
        public async Task RevokeAllByUserIdAsync_DoesNotAffectOtherUsers()
        {
            using var db = new TestDbContextWrapper();
            var user10 = MakeUser(10, "j@k.com");
            var user11 = MakeUser(11, "k@l.com");
            db.Context.Users.AddRange(user10, user11);
            db.Context.RefreshTokens.AddRange(
                new RefreshToken { Token = "tok-g", UserId = 10, User = user10, ExpiresAt = DateTime.UtcNow.AddDays(7), CreatedAt = DateTime.UtcNow },
                new RefreshToken { Token = "tok-h", UserId = 11, User = user11, ExpiresAt = DateTime.UtcNow.AddDays(7), CreatedAt = DateTime.UtcNow }
            );
            db.Context.SaveChanges();

            var repo = new RefreshTokenRepository(db.Context);
            await repo.RevokeAllByUserIdAsync(10);

            var otherUserToken = db.Context.RefreshTokens.First(t => t.Token == "tok-h");
            Assert.Null(otherUserToken.RevokedAt);
        }

        #endregion
    }
}
