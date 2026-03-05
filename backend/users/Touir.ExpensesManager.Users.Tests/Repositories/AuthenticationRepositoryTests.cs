using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Repositories;
using Touir.ExpensesManager.Users.Tests.Repositories.Helpers;

namespace Touir.ExpensesManager.Users.Tests.Repositories
{
    public class AuthenticationRepositoryTests
    {
        #region GetAuthenticationByUserIdAsync Tests

        [Fact]
        public async Task GetAuthenticationByUserIdAsync_ReturnsAuthentication_WhenExists()
        {
            using var db = new TestDbContextWrapper();
            var user = new User { Id = 1, FirstName = "A", LastName = "B", Email = "a@b.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var auth = new Authentication { UserId = 1, User = user, HashPassword = "hash", HashSalt = "salt", IsTemporaryPassword = false };
            db.Context.Users.Add(user);
            db.Context.Authentications.Add(auth);
            db.Context.SaveChanges();
            
            var repo = new AuthenticationRepository(db.Context);
            var result = await repo.GetAuthenticationByUserIdAsync(1);
            
            Assert.NotNull(result);
            Assert.Equal(1, result.UserId);
            Assert.Equal("hash", result.HashPassword);
            Assert.Equal("salt", result.HashSalt);
        }

        [Fact]
        public async Task GetAuthenticationByUserIdAsync_ReturnsNull_WhenNotExists()
        {
            using var db = new TestDbContextWrapper();
            var repo = new AuthenticationRepository(db.Context);
            
            var result = await repo.GetAuthenticationByUserIdAsync(999);
            
            Assert.Null(result);
        }

        #endregion

        #region CreateAuthenticationAsync Tests

        [Fact]
        public async Task CreateAuthenticationAsync_CreatesAuthentication_WhenValid()
        {
            using var db = new TestDbContextWrapper();
            var user = new User { Id = 2, FirstName = "C", LastName = "D", Email = "c@d.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            db.Context.Users.Add(user);
            db.Context.SaveChanges();
            
            var repo = new AuthenticationRepository(db.Context);
            var auth = new Authentication { UserId = 2, User = user, HashPassword = "hash2", HashSalt = "salt2", IsTemporaryPassword = true };
            var result = await repo.CreateAuthenticationAsync(auth);
            
            Assert.True(result);
            Assert.Single(db.Context.Authentications.Where(a => a.UserId == 2));
        }

        [Fact]
        public async Task CreateAuthenticationAsync_ResetsEmailValidationHash_WhenResetHashIsTrue()
        {
            using var db = new TestDbContextWrapper();
            var user = new User 
            { 
                Id = 3, 
                FirstName = "E", 
                LastName = "F", 
                Email = "e@f.com", 
                EmailValidationHash = "original-hash",
                CreatedAt = DateTime.UtcNow, 
                LastUpdatedAt = DateTime.UtcNow 
            };
            db.Context.Users.Add(user);
            db.Context.SaveChanges();
            
            var repo = new AuthenticationRepository(db.Context);
            var auth = new Authentication { UserId = 3, User = user, HashPassword = "hash3", HashSalt = "salt3", IsTemporaryPassword = false };
            var result = await repo.CreateAuthenticationAsync(auth, resetHash: true);
            
            Assert.True(result);
            var updatedUser = db.Context.Users.First(u => u.Id == 3);
            Assert.Null(updatedUser.EmailValidationHash);
        }

        [Fact]
        public async Task CreateAuthenticationAsync_DoesNotResetEmailValidationHash_WhenResetHashIsFalse()
        {
            using var db = new TestDbContextWrapper();
            var user = new User 
            { 
                Id = 4, 
                FirstName = "G", 
                LastName = "H", 
                Email = "g@h.com", 
                EmailValidationHash = "keep-this-hash",
                CreatedAt = DateTime.UtcNow, 
                LastUpdatedAt = DateTime.UtcNow 
            };
            db.Context.Users.Add(user);
            db.Context.SaveChanges();
            
            var repo = new AuthenticationRepository(db.Context);
            var auth = new Authentication { UserId = 4, User = user, HashPassword = "hash4", HashSalt = "salt4", IsTemporaryPassword = false };
            var result = await repo.CreateAuthenticationAsync(auth, resetHash: false);
            
            Assert.True(result);
            var updatedUser = db.Context.Users.First(u => u.Id == 4);
            Assert.Equal("keep-this-hash", updatedUser.EmailValidationHash);
        }

        [Fact]
        public async Task CreateAuthenticationAsync_ReturnsFalse_WhenAuthenticationIsNull()
        {
            using var db = new TestDbContextWrapper();
            var repo = new AuthenticationRepository(db.Context);
            
            var result = await repo.CreateAuthenticationAsync(null);
            
            Assert.False(result);
        }

        [Fact]
        public async Task CreateAuthenticationAsync_ReturnsFalse_WhenUserIsNull()
        {
            using var db = new TestDbContextWrapper();
            var repo = new AuthenticationRepository(db.Context);
            var auth = new Authentication { UserId = 5, User = null, HashPassword = "hash5", HashSalt = "salt5" };
            
            var result = await repo.CreateAuthenticationAsync(auth);
            
            Assert.False(result);
        }

        #endregion

        #region UpdateAuthenticationAsync Tests

        [Fact]
        public async Task UpdateAuthenticationAsync_UpdatesAuthentication_WhenValid()
        {
            using var db = new TestDbContextWrapper();
            var user = new User { Id = 6, FirstName = "I", LastName = "J", Email = "i@j.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var auth = new Authentication { UserId = 6, User = user, HashPassword = "oldhash", HashSalt = "oldsalt", IsTemporaryPassword = true };
            db.Context.Users.Add(user);
            db.Context.Authentications.Add(auth);
            db.Context.SaveChanges();
            
            var repo = new AuthenticationRepository(db.Context);
            var dbAuth = db.Context.Authentications.First(a => a.UserId == 6);
            dbAuth.HashPassword = "newhash";
            dbAuth.IsTemporaryPassword = false;
            
            var result = await repo.UpdateAuthenticationAsync(dbAuth);
            
            Assert.True(result);
            var updatedAuth = db.Context.Authentications.First(a => a.UserId == 6);
            Assert.Equal("newhash", updatedAuth.HashPassword);
            Assert.False(updatedAuth.IsTemporaryPassword);
        }

        [Fact]
        public async Task UpdateAuthenticationAsync_ResetsEmailValidationHash_WhenResetHashIsTrue()
        {
            using var db = new TestDbContextWrapper();
            var user = new User 
            { 
                Id = 7, 
                FirstName = "K", 
                LastName = "L", 
                Email = "k@l.com", 
                EmailValidationHash = "reset-me",
                CreatedAt = DateTime.UtcNow, 
                LastUpdatedAt = DateTime.UtcNow 
            };
            var auth = new Authentication { UserId = 7, User = user, HashPassword = "hash7", HashSalt = "salt7", IsTemporaryPassword = true };
            db.Context.Users.Add(user);
            db.Context.Authentications.Add(auth);
            db.Context.SaveChanges();
            
            var repo = new AuthenticationRepository(db.Context);
            var dbAuth = db.Context.Authentications.First(a => a.UserId == 7);
            dbAuth.HashPassword = "updatedhash";
            
            var result = await repo.UpdateAuthenticationAsync(dbAuth, resetHash: true);
            
            Assert.True(result);
            var updatedUser = db.Context.Users.First(u => u.Id == 7);
            Assert.Null(updatedUser.EmailValidationHash);
        }

        [Fact]
        public async Task UpdateAuthenticationAsync_DoesNotResetEmailValidationHash_WhenResetHashIsFalse()
        {
            using var db = new TestDbContextWrapper();
            var user = new User 
            { 
                Id = 8, 
                FirstName = "M", 
                LastName = "N", 
                Email = "m@n.com", 
                EmailValidationHash = "keep-me",
                CreatedAt = DateTime.UtcNow, 
                LastUpdatedAt = DateTime.UtcNow 
            };
            var auth = new Authentication { UserId = 8, User = user, HashPassword = "hash8", HashSalt = "salt8", IsTemporaryPassword = true };
            db.Context.Users.Add(user);
            db.Context.Authentications.Add(auth);
            db.Context.SaveChanges();
            
            var repo = new AuthenticationRepository(db.Context);
            var dbAuth = db.Context.Authentications.First(a => a.UserId == 8);
            dbAuth.HashPassword = "updatedhash";
            
            var result = await repo.UpdateAuthenticationAsync(dbAuth, resetHash: false);
            
            Assert.True(result);
            var updatedUser = db.Context.Users.First(u => u.Id == 8);
            Assert.Equal("keep-me", updatedUser.EmailValidationHash);
        }

        [Fact]
        public async Task UpdateAuthenticationAsync_ReturnsFalse_WhenAuthenticationIsNull()
        {
            using var db = new TestDbContextWrapper();
            var repo = new AuthenticationRepository(db.Context);
            
            var result = await repo.UpdateAuthenticationAsync(null);
            
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateAuthenticationAsync_ReturnsFalse_WhenUserIsNull()
        {
            using var db = new TestDbContextWrapper();
            var user = new User { Id = 9, FirstName = "O", LastName = "P", Email = "o@p.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var auth = new Authentication { UserId = 9, User = user, HashPassword = "hash9", HashSalt = "salt9" };
            db.Context.Users.Add(user);
            db.Context.Authentications.Add(auth);
            db.Context.SaveChanges();
            
            var repo = new AuthenticationRepository(db.Context);
            var dbAuth = db.Context.Authentications.First(a => a.UserId == 9);
            dbAuth.User = null; // Set user to null
            
            var result = await repo.UpdateAuthenticationAsync(dbAuth);
            
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateAuthenticationAsync_UpdatesPasswordResetFields()
        {
            using var db = new TestDbContextWrapper();
            var user = new User { Id = 10, FirstName = "Q", LastName = "R", Email = "q@r.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var auth = new Authentication 
            { 
                UserId = 10, 
                User = user, 
                HashPassword = "hash10", 
                HashSalt = "salt10",
                PasswordResetHash = null,
                PasswordResetRequestedAt = null
            };
            db.Context.Users.Add(user);
            db.Context.Authentications.Add(auth);
            db.Context.SaveChanges();
            
            var repo = new AuthenticationRepository(db.Context);
            var dbAuth = db.Context.Authentications.First(a => a.UserId == 10);
            var resetTime = DateTime.UtcNow;
            dbAuth.PasswordResetHash = "reset-hash-123";
            dbAuth.PasswordResetRequestedAt = resetTime;
            
            var result = await repo.UpdateAuthenticationAsync(dbAuth);
            
            Assert.True(result);
            var updatedAuth = db.Context.Authentications.First(a => a.UserId == 10);
            Assert.Equal("reset-hash-123", updatedAuth.PasswordResetHash);
            Assert.Equal(resetTime, updatedAuth.PasswordResetRequestedAt);
        }

        #endregion
    }
}
