using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Repositories;
using Touir.ExpensesManager.Users.Tests.Repositories.Helpers;

namespace Touir.ExpensesManager.Users.Tests.Repositories
{
    public class UserRepositoryTests
    {
        #region GetUserByEmailAsync Tests

        [Fact]
        public async Task GetUserByEmailAsync_ReturnsUser_WhenUserExists()
        {
            using var db = new TestDbContextWrapper();
            db.Context.Users.Add(new User { FirstName = "Jane", LastName = "Smith", Email = "jane@smith.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow });
            db.Context.SaveChanges();
            
            var repo = new UserRepository(db.Context);
            var user = await repo.GetUserByEmailAsync("jane@smith.com");
            
            Assert.NotNull(user);
            Assert.Equal("jane@smith.com", user.Email);
            Assert.Equal("Jane", user.FirstName);
            Assert.Equal("Smith", user.LastName);
        }

        [Fact]
        public async Task GetUserByEmailAsync_ReturnsUser_WhenEmailCaseIsDifferent()
        {
            using var db = new TestDbContextWrapper();
            db.Context.Users.Add(new User { FirstName = "John", LastName = "Doe", Email = "john@doe.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow });
            db.Context.SaveChanges();
            
            var repo = new UserRepository(db.Context);
            var user = await repo.GetUserByEmailAsync("JOHN@DOE.COM");
            
            Assert.NotNull(user);
            Assert.Equal("john@doe.com", user.Email);
        }

        [Fact]
        public async Task GetUserByEmailAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            using var db = new TestDbContextWrapper();
            var repo = new UserRepository(db.Context);
            
            var user = await repo.GetUserByEmailAsync("nonexistent@email.com");
            
            Assert.Null(user);
        }

        [Fact]
        public async Task GetUserByEmailAsync_ConvertsEmailToLowerCase()
        {
            using var db = new TestDbContextWrapper();
            db.Context.Users.Add(new User { FirstName = "Test", LastName = "User", Email = "test@example.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow });
            db.Context.SaveChanges();
            
            var repo = new UserRepository(db.Context);
            var user = await repo.GetUserByEmailAsync("Test@Example.COM");
            
            Assert.NotNull(user);
            Assert.Equal("test@example.com", user.Email);
        }

        #endregion

        #region CreateUserAsync Tests

        [Fact]
        public async Task CreateUserAsync_CreatesUser_WithValidData()
        {
            using var db = new TestDbContextWrapper();
            var user = new User { FirstName = "John", LastName = "Doe", Email = "john@doe.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            
            var repo = new UserRepository(db.Context);
            var result = await repo.CreateUserAsync(user);
            
            Assert.NotNull(result);
            Assert.Equal("john@doe.com", result.Email);
            Assert.Equal("John", result.FirstName);
            Assert.Equal("Doe", result.LastName);
        }

        [Fact]
        public async Task CreateUserAsync_ConvertsEmailToLowerCase()
        {
            using var db = new TestDbContextWrapper();
            var user = new User { FirstName = "Jane", LastName = "Smith", Email = "Jane@Smith.COM", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            
            var repo = new UserRepository(db.Context);
            var result = await repo.CreateUserAsync(user);
            
            Assert.NotNull(result);
            Assert.Equal("jane@smith.com", result.Email);
        }

        [Fact]
        public async Task CreateUserAsync_SavesUserToDatabase()
        {
            using var db = new TestDbContextWrapper();
            var user = new User { FirstName = "Mark", LastName = "Twain", Email = "mark@twain.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            
            var repo = new UserRepository(db.Context);
            await repo.CreateUserAsync(user);
            
            var savedUser = db.Context.Users.FirstOrDefault(u => u.Email == "mark@twain.com");
            Assert.NotNull(savedUser);
            Assert.Equal("Mark", savedUser.FirstName);
        }

        [Fact]
        public async Task CreateUserAsync_PreservesAllUserProperties()
        {
            using var db = new TestDbContextWrapper();
            var user = new User 
            { 
                FirstName = "Alice", 
                LastName = "Wonder", 
                Email = "alice@wonder.com",
                EmailValidationHash = "hash123",
                IsEmailValidated = false,
                IsDisabled = false,
                CreatedAt = DateTime.UtcNow, 
                LastUpdatedAt = DateTime.UtcNow 
            };
            
            var repo = new UserRepository(db.Context);
            var result = await repo.CreateUserAsync(user);
            
            Assert.Equal("alice@wonder.com", result.Email);
            Assert.Equal("hash123", result.EmailValidationHash);
            Assert.False(result.IsEmailValidated);
            Assert.False(result.IsDisabled);
        }

        #endregion

        #region DeleteUserAsync Tests

        [Fact]
        public async Task DeleteUserAsync_DeletesUser_WhenUserExists()
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
        public async Task DeleteUserAsync_ReturnsTrue_WhenUserDeleted()
        {
            using var db = new TestDbContextWrapper();
            var user = new User { FirstName = "Bob", LastName = "Builder", Email = "bob@builder.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            db.Context.Users.Add(user);
            db.Context.SaveChanges();
            
            var repo = new UserRepository(db.Context);
            var result = await repo.DeleteUserAsync(user);
            
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteUserAsync_RemovesUserFromDatabase()
        {
            using var db = new TestDbContextWrapper();
            var user = new User { FirstName = "Charlie", LastName = "Brown", Email = "charlie@brown.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            db.Context.Users.Add(user);
            db.Context.SaveChanges();
            var userId = user.Id;
            
            var repo = new UserRepository(db.Context);
            await repo.DeleteUserAsync(user);
            
            var deletedUser = db.Context.Users.FirstOrDefault(u => u.Id == userId);
            Assert.Null(deletedUser);
        }

        #endregion

        #region GetUsedEmailValidationHashesAsync Tests

        [Fact]
        public async Task GetUsedEmailValidationHashesAsync_ReturnsHashes_ForUnvalidatedEmailsOnly()
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
        public async Task GetUsedEmailValidationHashesAsync_ExcludesNullHashes()
        {
            using var db = new TestDbContextWrapper();
            db.Context.Users.Add(new User { FirstName = "A", LastName = "B", Email = "a@b.com", EmailValidationHash = null, IsEmailValidated = false, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow });
            db.Context.SaveChanges();
            
            var repo = new UserRepository(db.Context);
            var hashes = await repo.GetUsedEmailValidationHashesAsync();
            
            Assert.Empty(hashes);
        }

        [Fact]
        public async Task GetUsedEmailValidationHashesAsync_ExcludesValidatedEmails()
        {
            using var db = new TestDbContextWrapper();
            db.Context.Users.Add(new User { FirstName = "E", LastName = "F", Email = "e@f.com", EmailValidationHash = "hash-validated", IsEmailValidated = true, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow });
            db.Context.SaveChanges();
            
            var repo = new UserRepository(db.Context);
            var hashes = await repo.GetUsedEmailValidationHashesAsync();
            
            Assert.Empty(hashes);
        }

        [Fact]
        public async Task GetUsedEmailValidationHashesAsync_ReturnsMultipleHashes()
        {
            using var db = new TestDbContextWrapper();
            db.Context.Users.Add(new User { FirstName = "A", LastName = "B", Email = "a@b.com", EmailValidationHash = "hash1", IsEmailValidated = false, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow });
            db.Context.Users.Add(new User { FirstName = "C", LastName = "D", Email = "c@d.com", EmailValidationHash = "hash2", IsEmailValidated = false, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow });
            db.Context.Users.Add(new User { FirstName = "E", LastName = "F", Email = "e@f.com", EmailValidationHash = "hash3", IsEmailValidated = false, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow });
            db.Context.SaveChanges();
            
            var repo = new UserRepository(db.Context);
            var hashes = await repo.GetUsedEmailValidationHashesAsync();
            
            Assert.Equal(3, hashes.Count);
            Assert.Contains("hash1", hashes);
            Assert.Contains("hash2", hashes);
            Assert.Contains("hash3", hashes);
        }

        [Fact]
        public async Task GetUsedEmailValidationHashesAsync_ReturnsEmptyList_WhenNoUnvalidatedUsers()
        {
            using var db = new TestDbContextWrapper();
            db.Context.Users.Add(new User { FirstName = "A", LastName = "B", Email = "a@b.com", EmailValidationHash = "hash1", IsEmailValidated = true, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow });
            db.Context.SaveChanges();
            
            var repo = new UserRepository(db.Context);
            var hashes = await repo.GetUsedEmailValidationHashesAsync();
            
            Assert.Empty(hashes);
        }

        #endregion

        #region ValidateEmail Tests

        [Fact]
        public async Task ValidateEmail_ValidatesEmail_WhenHashAndEmailMatch()
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

        [Fact]
        public async Task ValidateEmail_ReturnsFalse_WhenUserNotFound()
        {
            using var db = new TestDbContextWrapper();
            var repo = new UserRepository(db.Context);
            
            var result = await repo.ValidateEmail("nonexistent", "nonexistent@email.com");
            
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateEmail_ReturnsFalse_WhenEmailDoesNotMatch()
        {
            using var db = new TestDbContextWrapper();
            db.Context.Users.Add(new User { FirstName = "I", LastName = "J", Email = "i@j.com", EmailValidationHash = "hash123", IsEmailValidated = false, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow });
            db.Context.SaveChanges();
            
            var repo = new UserRepository(db.Context);
            var result = await repo.ValidateEmail("hash123", "wrong@email.com");
            
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateEmail_ReturnsFalse_WhenHashDoesNotMatch()
        {
            using var db = new TestDbContextWrapper();
            db.Context.Users.Add(new User { FirstName = "K", LastName = "L", Email = "k@l.com", EmailValidationHash = "correcthash", IsEmailValidated = false, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow });
            db.Context.SaveChanges();
            
            var repo = new UserRepository(db.Context);
            var result = await repo.ValidateEmail("wronghash", "k@l.com");
            
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateEmail_HandlesCaseInsensitiveEmail()
        {
            using var db = new TestDbContextWrapper();
            db.Context.Users.Add(new User { FirstName = "M", LastName = "N", Email = "m@n.com", EmailValidationHash = "hash456", IsEmailValidated = false, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow });
            db.Context.SaveChanges();
            
            var repo = new UserRepository(db.Context);
            var result = await repo.ValidateEmail("hash456", "M@N.COM");
            
            Assert.True(result);
            var user = db.Context.Users.First(u => u.Email == "m@n.com");
            Assert.True(user.IsEmailValidated);
        }

        [Fact]
        public async Task ValidateEmail_SetsIsEmailValidatedToTrue()
        {
            using var db = new TestDbContextWrapper();
            var user = new User { FirstName = "O", LastName = "P", Email = "o@p.com", EmailValidationHash = "hash789", IsEmailValidated = false, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            db.Context.Users.Add(user);
            db.Context.SaveChanges();
            
            var repo = new UserRepository(db.Context);
            await repo.ValidateEmail("hash789", "o@p.com");
            
            var updatedUser = db.Context.Users.First(u => u.Email == "o@p.com");
            Assert.True(updatedUser.IsEmailValidated);
        }

        [Fact]
        public async Task ValidateEmail_SavesChangesToDatabase()
        {
            using var db = new TestDbContextWrapper();
            db.Context.Users.Add(new User { FirstName = "Q", LastName = "R", Email = "q@r.com", EmailValidationHash = "savehash", IsEmailValidated = false, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow });
            db.Context.SaveChanges();
            
            var repo = new UserRepository(db.Context);
            await repo.ValidateEmail("savehash", "q@r.com");
            
            var updatedUser = db.Context.Users.First(u => u.Email == "q@r.com");
            Assert.True(updatedUser.IsEmailValidated);
        }

        #endregion
    }
}
