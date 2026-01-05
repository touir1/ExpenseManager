using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Repositories;
using Touir.ExpensesManager.Users.Tests.Repositories.Helpers;

namespace Touir.ExpensesManager.Users.Tests.Repositories
{
    public class UserRepositoryTests
    {
        [Fact]
        public async Task CreateUserAsync_CreatesUser()
        {
            using var db = new TestDbContextWrapper();
            var user = new User { FirstName = "John", LastName = "Doe", Email = "john@doe.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            
            var repo = new UserRepository(db.Context);
            var result = await repo.CreateUserAsync(user);
            
            Assert.NotNull(result);
            Assert.Equal("john@doe.com", result.Email);
        }

        [Fact]
        public async Task GetUserByEmailAsync_ReturnsUser()
        {
            using var db = new TestDbContextWrapper();
            db.Context.Users.Add(new User { FirstName = "Jane", LastName = "Smith", Email = "jane@smith.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow });
            db.Context.SaveChanges();
            
            var repo = new UserRepository(db.Context);
            var user = await repo.GetUserByEmailAsync("jane@smith.com");
            
            Assert.NotNull(user);
            Assert.Equal("jane@smith.com", user.Email);
        }

        [Fact]
        public async Task DeleteUserAsync_DeletesUser()
        {
            using var db = new TestDbContextWrapper();
            var user = new User { FirstName = "Mark", LastName = "Twain", Email = "mark@twain.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            db.Context.Users.Add(user);
            db.Context.SaveChanges();
            
            var repo = new UserRepository(db.Context);
            var dbUser = db.Context.Users.First();
            var result = await repo.DeleteUserAsync(dbUser);
            
            Assert.True(result);
            Assert.Empty(db.Context.Users);
        }

        [Fact]
        public async Task GetUsedEmailValidationHashesAsync_ReturnsHashes()
        {
            using var db = new TestDbContextWrapper();
            db.Context.Users.Add(new User { FirstName = "A", LastName = "B", Email = "a@b.com", EmailValidationHash = "hash1", IsEmailValidated = false, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow });
            db.Context.Users.Add(new User { FirstName = "C", LastName = "D", Email = "c@d.com", EmailValidationHash = null, IsEmailValidated = false, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow });
            db.Context.Users.Add(new User { FirstName = "E", LastName = "F", Email = "e@f.com", EmailValidationHash = "hash2", IsEmailValidated = true, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow });
            db.Context.SaveChanges();
            
            var repo = new UserRepository(db.Context);
            var hashes = await repo.GetUsedEmailValidationHashesAsync();
            
            Assert.Single(hashes);
            Assert.Contains("hash1", hashes);
        }

        [Fact]
        public async Task ValidateEmail_ValidatesEmail()
        {
            using var db = new TestDbContextWrapper();
            db.Context.Users.Add(new User { FirstName = "G", LastName = "H", Email = "g@h.com", EmailValidationHash = "valhash", IsEmailValidated = false, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow });
            db.Context.SaveChanges();
            
            var repo = new UserRepository(db.Context);
            var result = await repo.ValidateEmail("valhash", "g@h.com");
            
            Assert.True(result);
            var user = db.Context.Users.First(u => u.Email == "g@h.com");
            Assert.True(user.IsEmailValidated);
        }
    }
}
