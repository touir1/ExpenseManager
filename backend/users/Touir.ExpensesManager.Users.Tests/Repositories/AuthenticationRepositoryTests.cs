using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Repositories;
using Touir.ExpensesManager.Users.Tests.Repositories.Helpers;

namespace Touir.ExpensesManager.Users.Tests.Repositories
{
    public class AuthenticationRepositoryTests
    {
        [Fact]
        public async Task GetAuthenticationByUserIdAsync_ReturnsAuthentication()
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
        }

        [Fact]
        public async Task CreateAuthenticationAsync_CreatesAuthentication()
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
        public async Task UpdateAuthenticationAsync_UpdatesAuthentication()
        {
            using var db = new TestDbContextWrapper();
            var user = new User { Id = 3, FirstName = "E", LastName = "F", Email = "e@f.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var auth = new Authentication { UserId = 3, User = user, HashPassword = "oldhash", HashSalt = "oldsalt", IsTemporaryPassword = true };
            db.Context.Users.Add(user);
            db.Context.Authentications.Add(auth);
            db.Context.SaveChanges();
            
            var repo = new AuthenticationRepository(db.Context);
            var dbAuth = db.Context.Authentications.First(a => a.UserId == 3);
            dbAuth.HashPassword = "newhash";
            
            var result = await repo.UpdateAuthenticationAsync(dbAuth);
            Assert.True(result);
            Assert.Equal("newhash", db.Context.Authentications.First(a => a.UserId == 3).HashPassword);
        }
    }
}
