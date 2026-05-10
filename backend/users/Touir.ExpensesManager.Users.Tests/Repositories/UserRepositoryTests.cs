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
            
            Assert.NotNull(result);
            Assert.Equal("alice@wonder.com", result.Email);
            Assert.Equal("hash123", result.EmailValidationHash);
            Assert.False(result.IsEmailValidated);
            Assert.False(result.IsDisabled);
        }

        #endregion

        #region DeleteUserAsync Tests

        [Fact]
        public async Task DeleteUserAsync_SoftDeletesUser_WhenUserExists()
        {
            using var db = new TestDbContextWrapper();
            var user = new User { FirstName = "Mark", LastName = "Twain", Email = "mark@twain.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            db.Context.Users.Add(user);
            db.Context.SaveChanges();

            var repo = new UserRepository(db.Context);
            var dbUser = db.Context.Users.First();
            var result = await repo.DeleteUserAsync(dbUser);

            Assert.True(result);
            var deletedUser = db.Context.Users.First(u => u.Id == dbUser.Id);
            Assert.True(deletedUser.IsDeleted);
            Assert.NotNull(deletedUser.DeletedAt);
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
        public async Task DeleteUserAsync_UserStillExistsInDatabase_AfterSoftDelete()
        {
            using var db = new TestDbContextWrapper();
            var user = new User { FirstName = "Charlie", LastName = "Brown", Email = "charlie@brown.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            db.Context.Users.Add(user);
            db.Context.SaveChanges();
            var userId = user.Id;

            var repo = new UserRepository(db.Context);
            await repo.DeleteUserAsync(user);

            var deletedUser = db.Context.Users.FirstOrDefault(u => u.Id == userId);
            Assert.NotNull(deletedUser);
            Assert.True(deletedUser.IsDeleted);
        }

        [Fact]
        public async Task DeleteUserAsync_HidesUserFromGetByEmail_AfterSoftDelete()
        {
            using var db = new TestDbContextWrapper();
            var user = new User { FirstName = "Dave", LastName = "Deleted", Email = "dave@deleted.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            db.Context.Users.Add(user);
            db.Context.SaveChanges();

            var repo = new UserRepository(db.Context);
            await repo.DeleteUserAsync(user);

            var found = await repo.GetUserByEmailAsync("dave@deleted.com");
            Assert.Null(found);
        }

        [Fact]
        public async Task DeleteUserAsync_HidesUserFromGetById_AfterSoftDelete()
        {
            using var db = new TestDbContextWrapper();
            var user = new User { FirstName = "Eve", LastName = "Erased", Email = "eve@erased.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            db.Context.Users.Add(user);
            db.Context.SaveChanges();
            var userId = user.Id;

            var repo = new UserRepository(db.Context);
            await repo.DeleteUserAsync(user);

            var found = await repo.GetUserByIdAsync(userId);
            Assert.Null(found);
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

        #region UpdateEmailValidationHashAsync Tests

        [Fact]
        public async Task UpdateEmailValidationHashAsync_UpdatesHash_WhenUserExists()
        {
            using var db = new TestDbContextWrapper();
            var user = new User { FirstName = "A", LastName = "B", Email = "a@b.com", EmailValidationHash = "oldhash", IsEmailValidated = false, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            db.Context.Users.Add(user);
            db.Context.SaveChanges();

            var repo = new UserRepository(db.Context);
            var expiry = DateTime.UtcNow.AddHours(24);
            await repo.UpdateEmailValidationHashAsync(user.Id, "newhash", expiry);

            var updated = db.Context.Users.First(u => u.Email == "a@b.com");
            Assert.Equal("newhash", updated.EmailValidationHash);
        }

        [Fact]
        public async Task UpdateEmailValidationHashAsync_UpdatesExpiresAt_WhenUserExists()
        {
            using var db = new TestDbContextWrapper();
            var user = new User { FirstName = "C", LastName = "D", Email = "c@d.com", EmailValidationHash = "oldhash", IsEmailValidated = false, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            db.Context.Users.Add(user);
            db.Context.SaveChanges();

            var repo = new UserRepository(db.Context);
            var expiry = DateTime.UtcNow.AddHours(24);
            await repo.UpdateEmailValidationHashAsync(user.Id, "newhash", expiry);

            var updated = db.Context.Users.First(u => u.Email == "c@d.com");
            Assert.NotNull(updated.EmailValidationHashExpiresAt);
            Assert.True(updated.EmailValidationHashExpiresAt >= expiry.AddSeconds(-1) && updated.EmailValidationHashExpiresAt <= expiry.AddSeconds(1));
        }

        [Fact]
        public async Task UpdateEmailValidationHashAsync_DoesNothing_WhenUserNotFound()
        {
            using var db = new TestDbContextWrapper();
            var repo = new UserRepository(db.Context);

            await repo.UpdateEmailValidationHashAsync(9999, "newhash", DateTime.UtcNow.AddHours(24));

            Assert.Empty(db.Context.Users.ToList());
        }

        [Fact]
        public async Task UpdateEmailValidationHashAsync_DoesNothing_WhenUserSoftDeleted()
        {
            using var db = new TestDbContextWrapper();
            var user = new User { FirstName = "E", LastName = "F", Email = "e@f.com", EmailValidationHash = "oldhash", IsEmailValidated = false, IsDeleted = true, DeletedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            db.Context.Users.Add(user);
            db.Context.SaveChanges();

            var repo = new UserRepository(db.Context);
            await repo.UpdateEmailValidationHashAsync(user.Id, "newhash", DateTime.UtcNow.AddHours(24));

            var unchanged = db.Context.Users.First(u => u.Email == "e@f.com");
            Assert.Equal("oldhash", unchanged.EmailValidationHash);
        }

        #endregion

        #region ValidateEmailAsync Tests

        [Fact]
        public async Task ValidateEmailAsync_ReturnsUser_WhenHashAndEmailMatch()
        {
            using var db = new TestDbContextWrapper();
            db.Context.Users.Add(new User { FirstName = "S", LastName = "T", Email = "s@t.com", EmailValidationHash = "hash-s", IsEmailValidated = false, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow });
            db.Context.SaveChanges();

            var repo = new UserRepository(db.Context);
            var user = await repo.ValidateEmailAsync("hash-s", "s@t.com");

            Assert.NotNull(user);
            Assert.Equal("s@t.com", user.Email);
        }

        [Fact]
        public async Task ValidateEmailAsync_SetsIsEmailValidated()
        {
            using var db = new TestDbContextWrapper();
            db.Context.Users.Add(new User { FirstName = "U", LastName = "V", Email = "u@v.com", EmailValidationHash = "hash-u", IsEmailValidated = false, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow });
            db.Context.SaveChanges();

            var repo = new UserRepository(db.Context);
            await repo.ValidateEmailAsync("hash-u", "u@v.com");

            var updated = db.Context.Users.First(u => u.Email == "u@v.com");
            Assert.True(updated.IsEmailValidated);
        }

        [Fact]
        public async Task ValidateEmailAsync_ReturnsNull_WhenUserNotFound()
        {
            using var db = new TestDbContextWrapper();
            var repo = new UserRepository(db.Context);

            var user = await repo.ValidateEmailAsync("no-hash", "none@none.com");

            Assert.Null(user);
        }

        [Fact]
        public async Task ValidateEmailAsync_ReturnsNull_WhenHashDoesNotMatch()
        {
            using var db = new TestDbContextWrapper();
            db.Context.Users.Add(new User { FirstName = "W", LastName = "X", Email = "w@x.com", EmailValidationHash = "real-hash", IsEmailValidated = false, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow });
            db.Context.SaveChanges();

            var repo = new UserRepository(db.Context);
            var user = await repo.ValidateEmailAsync("wrong-hash", "w@x.com");

            Assert.Null(user);
        }

        [Fact]
        public async Task ValidateEmailAsync_IsCaseInsensitiveOnEmail()
        {
            using var db = new TestDbContextWrapper();
            db.Context.Users.Add(new User { FirstName = "Y", LastName = "Z", Email = "y@z.com", EmailValidationHash = "hash-y", IsEmailValidated = false, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow });
            db.Context.SaveChanges();

            var repo = new UserRepository(db.Context);
            var user = await repo.ValidateEmailAsync("hash-y", "Y@Z.COM");

            Assert.NotNull(user);
        }

        [Fact]
        public async Task ValidateEmailAsync_ReturnsNull_WhenHashIsExpired()
        {
            using var db = new TestDbContextWrapper();
            db.Context.Users.Add(new User { FirstName = "A", LastName = "B", Email = "a@b.com", EmailValidationHash = "expiredhash", EmailValidationHashExpiresAt = DateTime.UtcNow.AddHours(-1), IsEmailValidated = false, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow });
            db.Context.SaveChanges();

            var repo = new UserRepository(db.Context);
            var user = await repo.ValidateEmailAsync("expiredhash", "a@b.com");

            Assert.Null(user);
        }

        [Fact]
        public async Task ValidateEmailAsync_ReturnsUser_WhenHashNotYetExpired()
        {
            using var db = new TestDbContextWrapper();
            db.Context.Users.Add(new User { FirstName = "C", LastName = "D", Email = "c@d.com", EmailValidationHash = "validhash", EmailValidationHashExpiresAt = DateTime.UtcNow.AddHours(23), IsEmailValidated = false, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow });
            db.Context.SaveChanges();

            var repo = new UserRepository(db.Context);
            var user = await repo.ValidateEmailAsync("validhash", "c@d.com");

            Assert.NotNull(user);
        }

        [Fact]
        public async Task ValidateEmailAsync_ReturnsUser_WhenNoExpirySet()
        {
            using var db = new TestDbContextWrapper();
            db.Context.Users.Add(new User { FirstName = "E", LastName = "F", Email = "e@f.com", EmailValidationHash = "nolimithash", EmailValidationHashExpiresAt = null, IsEmailValidated = false, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow });
            db.Context.SaveChanges();

            var repo = new UserRepository(db.Context);
            var user = await repo.ValidateEmailAsync("nolimithash", "e@f.com");

            Assert.NotNull(user);
        }

        #endregion
    }
}
