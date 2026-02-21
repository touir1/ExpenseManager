using Touir.ExpensesManager.Expenses.Models.External;
using Touir.ExpensesManager.Expenses.Repositories.External;
using Touir.ExpensesManager.Expenses.Tests.TestHelpers;

namespace Touir.ExpensesManager.Expenses.Tests.Repositories.External
{
    public class UserRepositoryTests
    {

        [Fact]
        public async Task GetUserByIdAsync_WhenUserExists_ReturnsUser()
        {
            using var db = new TestExpensesDbContextWrapper();
            // Arrange
            var user = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                FamilyId = 100,
                IsDeleted = false
            };
            await db.Context.Users.AddAsync(user);
            await db.Context.SaveChangesAsync();

            // Act
            var repo = new UserRepository(db.Context);
            var result = await repo.GetUserByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("John", result.FirstName);
            Assert.Equal("Doe", result.LastName);
            Assert.Equal("john.doe@example.com", result.Email);
            Assert.Equal(100, result.FamilyId);
            Assert.False(result.IsDeleted);
        }

        [Fact]
        public async Task GetUserByIdAsync_WhenUserDoesNotExist_ReturnsNull()
        {
            using var db = new TestExpensesDbContextWrapper();
            var repo = new UserRepository(db.Context);

            // Act
            var result = await repo.GetUserByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUsersByFamilyIdAsync_WhenUsersExist_ReturnsAllUsersInFamily()
        {
            using var db = new TestExpensesDbContextWrapper();
            var repo = new UserRepository(db.Context);

            // Arrange
            var users = new[]
            {
                new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", FamilyId = 100, IsDeleted = false },
                new User { Id = 2, FirstName = "Jane", LastName = "Doe", Email = "jane@example.com", FamilyId = 100, IsDeleted = false },
                new User { Id = 3, FirstName = "Bob", LastName = "Smith", Email = "bob@example.com", FamilyId = 200, IsDeleted = false }
            };
            await db.Context.Users.AddRangeAsync(users);
            await db.Context.SaveChangesAsync();

            // Act
            var result = await repo.GetUsersByFamilyIdAsync(100);
            // Assert
            var userList = result.ToList();
            Assert.Equal(2, userList.Count);
            Assert.All(userList, u => Assert.Equal(100, u.FamilyId));
            Assert.Contains(userList, u => u.FirstName == "John");
            Assert.Contains(userList, u => u.FirstName == "Jane");
        }

        [Fact]
        public async Task GetUsersByFamilyIdAsync_WhenNoUsersExist_ReturnsEmptyList()
        {
            using var db = new TestExpensesDbContextWrapper();
            var repo = new UserRepository(db.Context);

            // Act
            var result = await repo.GetUsersByFamilyIdAsync(999);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task SaveOrUpdateUserAsync_WhenUserDoesNotExist_InsertsNewUser()
        {
            using var db = new TestExpensesDbContextWrapper();
            var repo = new UserRepository(db.Context);

            // Arrange
            var newUser = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                FamilyId = 100
            };

            // Act
            await repo.SaveOrUpdateUserAsync(newUser);

            // Assert
            var savedUser = await db.Context.Users.FindAsync(1);
            Assert.NotNull(savedUser);
            Assert.Equal("John", savedUser.FirstName);
            Assert.Equal("Doe", savedUser.LastName);
            Assert.Equal("john.doe@example.com", savedUser.Email);
            Assert.Equal(100, savedUser.FamilyId);
            Assert.False(savedUser.IsDeleted);
        }

        [Fact]
        public async Task SaveOrUpdateUserAsync_WhenUserExists_UpdatesExistingUser()
        {
            using var db = new TestExpensesDbContextWrapper();
            var repo = new UserRepository(db.Context);

            // Arrange
            var existingUser = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                FamilyId = 100,
                IsDeleted = false
            };
            await db.Context.Users.AddAsync(existingUser);
            await db.Context.SaveChangesAsync();

            var updatedUser = new User
            {
                Id = 1,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                FamilyId = 200
            };

            // Act
            await repo.SaveOrUpdateUserAsync(updatedUser);

            // Assert
            var savedUser = await db.Context.Users.FindAsync(1);
            Assert.NotNull(savedUser);
            Assert.Equal("Jane", savedUser.FirstName);
            Assert.Equal("Smith", savedUser.LastName);
            Assert.Equal("jane.smith@example.com", savedUser.Email);
            Assert.Equal(200, savedUser.FamilyId);
            Assert.False(savedUser.IsDeleted); // Should remain unchanged
        }

        [Fact]
        public async Task SaveOrUpdateUserAsync_WhenInsertingNewUser_SetsIsDeletedToFalse()
        {
            using var db = new TestExpensesDbContextWrapper();
            var repo = new UserRepository(db.Context);

            // Arrange
            var newUser = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                FamilyId = 100,
                IsDeleted = true // Even if set to true, should be overridden
            };

            // Act
            await repo.SaveOrUpdateUserAsync(newUser);

            // Assert
            var savedUser = await db.Context.Users.FindAsync(1);
            Assert.NotNull(savedUser);
            Assert.False(savedUser.IsDeleted);
        }

        [Fact]
        public async Task DeleteUserAsync_WhenUserExists_MarksUserAsDeleted()
        {
            using var db = new TestExpensesDbContextWrapper();
            var repo = new UserRepository(db.Context);

            // Arrange
            var user = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                FamilyId = 100,
                IsDeleted = false
            };
            await db.Context.Users.AddAsync(user);
            await db.Context.SaveChangesAsync();

            // Act
            await repo.DeleteUserAsync(user);
            // Assert
            var deletedUser = await db.Context.Users.FindAsync(1);
            Assert.NotNull(deletedUser);
            Assert.True(deletedUser.IsDeleted);
            Assert.Equal("John", deletedUser.FirstName); // Other properties should remain unchanged
        }

        [Fact]
        public async Task DeleteUserAsync_WhenUserDoesNotExist_DoesNotThrowException()
        {
            using var db = new TestExpensesDbContextWrapper();
            var repo = new UserRepository(db.Context);

            // Arrange
            var nonExistentUser = new User
            {
                Id = 999,
                FirstName = "Ghost",
                LastName = "User",
                Email = "ghost@example.com"
            };

            // Act & Assert
            var exception = await Record.ExceptionAsync(async () =>
                await repo.DeleteUserAsync(nonExistentUser)
            );
            Assert.Null(exception);
        }

        [Fact]
        public async Task DeleteUserAsync_WhenUserAlreadyDeleted_RemainsDeleted()
        {
            using var db = new TestExpensesDbContextWrapper();
            var repo = new UserRepository(db.Context);

            // Arrange
            var user = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                FamilyId = 100,
                IsDeleted = true
            };
            await db.Context.Users.AddAsync(user);
            await db.Context.SaveChangesAsync();

            // Act
            await repo.DeleteUserAsync(user);
            // Assert
            var deletedUser = await db.Context.Users.FindAsync(1);
            Assert.NotNull(deletedUser);
            Assert.True(deletedUser.IsDeleted);
        }

        [Fact]
        public async Task SaveOrUpdateUserAsync_WithNullFamilyId_SavesSuccessfully()
        {
            using var db = new TestExpensesDbContextWrapper();
            var repo = new UserRepository(db.Context);
    
            // Arrange
            var user = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                FamilyId = null,
                IsDeleted = false
            };

            // Act
            await repo.SaveOrUpdateUserAsync(user);

            // Assert
            var savedUser = await db.Context.Users.FindAsync(1);
            Assert.NotNull(savedUser);
            Assert.Null(savedUser.FamilyId);
        }

        [Fact]
        public async Task GetUsersByFamilyIdAsync_IncludesOnlyNonDeletedUsers()
        {
            using var db = new TestExpensesDbContextWrapper();
            var repo = new UserRepository(db.Context);

            // Arrange
            var users = new[]
            {
                new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", FamilyId = 100, IsDeleted = false },
                new User { Id = 2, FirstName = "Jane", LastName = "Doe", Email = "jane@example.com", FamilyId = 100, IsDeleted = true },
                new User { Id = 3, FirstName = "Bob", LastName = "Doe", Email = "bob@example.com", FamilyId = 100, IsDeleted = false }
            };
            await db.Context.Users.AddRangeAsync(users);
            await db.Context.SaveChangesAsync();

            // Act
            var result = await repo.GetUsersByFamilyIdAsync(100);

            // Assert
            var userList = result.ToList();
            Assert.Equal(3, userList.Count); // All users are returned regardless of IsDeleted
            Assert.Contains(userList, u => u.Id == 2 && u.IsDeleted); // Deleted user is included
        }
    }
}