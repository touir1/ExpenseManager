using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Repositories;
using Touir.ExpensesManager.Users.Tests.Repositories.Helpers;

namespace Touir.ExpensesManager.Users.Tests.Repositories
{
    public class RoleRepositoryTests
    {
        #region GetUserRolesByApplicationCodeAsync Tests

        [Fact]
        public async Task GetUserRolesByApplicationCodeAsync_ReturnsRoles_WhenUserHasRoles()
        {
            using var db = new TestDbContextWrapper();
            var app = new Application { Code = "APP1", Name = "App1" };
            var user = new User { FirstName = "A", LastName = "B", Email = "a@b.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var role = new Role { Code = "ADMIN", Name = "Admin", Application = app };
            db.Context.Applications.Add(app);
            db.Context.Users.Add(user);
            db.Context.Roles.Add(role);
            db.Context.SaveChanges();
            
            var userRole = new UserRole { UserId = user.Id, RoleId = role.Id, User = user, Role = role, CreatedAt = DateTime.UtcNow };
            db.Context.UserRoles.Add(userRole);
            db.Context.SaveChanges();
            
            var repo = new RoleRepository(db.Context);
            var roles = await repo.GetUserRolesByApplicationCodeAsync("APP1", user.Id);
            
            Assert.Single(roles);
            Assert.Equal("ADMIN", roles.First().Code);
            Assert.Equal("Admin", roles.First().Name);
        }

        [Fact]
        public async Task GetUserRolesByApplicationCodeAsync_ReturnsMultipleRoles_WhenUserHasMultipleRoles()
        {
            using var db = new TestDbContextWrapper();
            var app = new Application { Code = "APP2", Name = "App2" };
            var user = new User { FirstName = "C", LastName = "D", Email = "c@d.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var role1 = new Role { Code = "ADMIN", Name = "Admin", Application = app };
            var role2 = new Role { Code = "USER", Name = "User", Application = app };
            db.Context.Applications.Add(app);
            db.Context.Users.Add(user);
            db.Context.Roles.AddRange(role1, role2);
            db.Context.SaveChanges();
            
            var userRole1 = new UserRole { UserId = user.Id, RoleId = role1.Id, User = user, Role = role1, CreatedAt = DateTime.UtcNow };
            var userRole2 = new UserRole { UserId = user.Id, RoleId = role2.Id, User = user, Role = role2, CreatedAt = DateTime.UtcNow };
            db.Context.UserRoles.AddRange(userRole1, userRole2);
            db.Context.SaveChanges();
            
            var repo = new RoleRepository(db.Context);
            var roles = await repo.GetUserRolesByApplicationCodeAsync("APP2", user.Id);
            
            Assert.Equal(2, roles.Count());
            Assert.Contains(roles, r => r.Code == "ADMIN");
            Assert.Contains(roles, r => r.Code == "USER");
        }

        [Fact]
        public async Task GetUserRolesByApplicationCodeAsync_ReturnsEmpty_WhenUserHasNoRoles()
        {
            using var db = new TestDbContextWrapper();
            var app = new Application { Code = "APP3", Name = "App3" };
            var user = new User { FirstName = "E", LastName = "F", Email = "e@f.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            db.Context.Applications.Add(app);
            db.Context.Users.Add(user);
            db.Context.SaveChanges();
            
            var repo = new RoleRepository(db.Context);
            var roles = await repo.GetUserRolesByApplicationCodeAsync("APP3", user.Id);
            
            Assert.Empty(roles);
        }

        [Fact]
        public async Task GetUserRolesByApplicationCodeAsync_ReturnsEmpty_WhenApplicationCodeDoesNotExist()
        {
            using var db = new TestDbContextWrapper();
            var app = new Application { Code = "APP4", Name = "App4" };
            var user = new User { FirstName = "G", LastName = "H", Email = "g@h.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var role = new Role { Code = "ADMIN", Name = "Admin", Application = app };
            db.Context.Applications.Add(app);
            db.Context.Users.Add(user);
            db.Context.Roles.Add(role);
            db.Context.SaveChanges();
            
            var userRole = new UserRole { UserId = user.Id, RoleId = role.Id, User = user, Role = role, CreatedAt = DateTime.UtcNow };
            db.Context.UserRoles.Add(userRole);
            db.Context.SaveChanges();
            
            var repo = new RoleRepository(db.Context);
            var roles = await repo.GetUserRolesByApplicationCodeAsync("NONEXISTENT", user.Id);
            
            Assert.Empty(roles);
        }

        [Fact]
        public async Task GetUserRolesByApplicationCodeAsync_ReturnsEmpty_WhenUserDoesNotExist()
        {
            using var db = new TestDbContextWrapper();
            var app = new Application { Code = "APP5", Name = "App5" };
            var role = new Role { Code = "ADMIN", Name = "Admin", Application = app };
            db.Context.Applications.Add(app);
            db.Context.Roles.Add(role);
            db.Context.SaveChanges();
            
            var repo = new RoleRepository(db.Context);
            var roles = await repo.GetUserRolesByApplicationCodeAsync("APP5", 999);
            
            Assert.Empty(roles);
        }

        [Fact]
        public async Task GetUserRolesByApplicationCodeAsync_OnlyReturnsRolesForSpecifiedApplication()
        {
            using var db = new TestDbContextWrapper();
            var app1 = new Application { Code = "APP6", Name = "App6" };
            var app2 = new Application { Code = "APP7", Name = "App7" };
            var user = new User { FirstName = "I", LastName = "J", Email = "i@j.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var role1 = new Role { Code = "ADMIN1", Name = "Admin1", Application = app1 };
            var role2 = new Role { Code = "ADMIN2", Name = "Admin2", Application = app2 };
            db.Context.Applications.AddRange(app1, app2);
            db.Context.Users.Add(user);
            db.Context.Roles.AddRange(role1, role2);
            db.Context.SaveChanges();
            
            var userRole1 = new UserRole { UserId = user.Id, RoleId = role1.Id, User = user, Role = role1, CreatedAt = DateTime.UtcNow };
            var userRole2 = new UserRole { UserId = user.Id, RoleId = role2.Id, User = user, Role = role2, CreatedAt = DateTime.UtcNow };
            db.Context.UserRoles.AddRange(userRole1, userRole2);
            db.Context.SaveChanges();
            
            var repo = new RoleRepository(db.Context);
            var roles = await repo.GetUserRolesByApplicationCodeAsync("APP6", user.Id);
            
            Assert.Single(roles);
            Assert.Equal("ADMIN1", roles.First().Code);
        }

        #endregion

        #region GetDefaultRoleByApplicationIdAsync Tests

        [Fact]
        public async Task GetDefaultRoleByApplicationIdAsync_ReturnsDefaultRole_WhenExists()
        {
            using var db = new TestDbContextWrapper();
            var app = new Application { Code = "APP8", Name = "App8" };
            var role = new Role { Code = "USER", Name = "User", Application = app, IsDefault = true };
            db.Context.Applications.Add(app);
            db.Context.Roles.Add(role);
            db.Context.SaveChanges();
            
            var repo = new RoleRepository(db.Context);
            var result = await repo.GetDefaultRoleByApplicationIdAsync(app.Id);
            
            Assert.NotNull(result);
            Assert.True(result.IsDefault);
            Assert.Equal("USER", result.Code);
        }

        [Fact]
        public async Task GetDefaultRoleByApplicationIdAsync_ReturnsNull_WhenNoDefaultRole()
        {
            using var db = new TestDbContextWrapper();
            var app = new Application { Code = "APP9", Name = "App9" };
            var role = new Role { Code = "USER", Name = "User", Application = app, IsDefault = false };
            db.Context.Applications.Add(app);
            db.Context.Roles.Add(role);
            db.Context.SaveChanges();
            
            var repo = new RoleRepository(db.Context);
            var result = await repo.GetDefaultRoleByApplicationIdAsync(app.Id);
            
            Assert.Null(result);
        }

        [Fact]
        public async Task GetDefaultRoleByApplicationIdAsync_ReturnsNull_WhenApplicationDoesNotExist()
        {
            using var db = new TestDbContextWrapper();
            var repo = new RoleRepository(db.Context);
            
            var result = await repo.GetDefaultRoleByApplicationIdAsync(999);
            
            Assert.Null(result);
        }

        [Fact]
        public async Task GetDefaultRoleByApplicationIdAsync_ReturnsFirstDefault_WhenMultipleDefaultsExist()
        {
            using var db = new TestDbContextWrapper();
            var app = new Application { Code = "APP10", Name = "App10" };
            var role1 = new Role { Code = "DEFAULT1", Name = "Default1", Application = app, IsDefault = true };
            var role2 = new Role { Code = "DEFAULT2", Name = "Default2", Application = app, IsDefault = true };
            db.Context.Applications.Add(app);
            db.Context.Roles.AddRange(role1, role2);
            db.Context.SaveChanges();
            
            var repo = new RoleRepository(db.Context);
            var result = await repo.GetDefaultRoleByApplicationIdAsync(app.Id);
            
            Assert.NotNull(result);
            Assert.True(result.IsDefault);
        }

        #endregion

        #region AssignRoleToUserAsync Tests

        [Fact]
        public async Task AssignRoleToUserAsync_AssignsRole_WhenValid()
        {
            using var db = new TestDbContextWrapper();
            var user = new User { FirstName = "K", LastName = "L", Email = "k@l.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var app = new Application { Code = "APP11", Name = "App11" };
            var role = new Role { Code = "MOD", Name = "Moderator", Application = app };
            db.Context.Users.Add(user);
            db.Context.Applications.Add(app);
            db.Context.Roles.Add(role);
            db.Context.SaveChanges();
            
            var repo = new RoleRepository(db.Context);
            var result = await repo.AssignRoleToUserAsync(role.Id, user.Id);
            
            Assert.True(result);
            var userRole = db.Context.UserRoles.FirstOrDefault(ur => ur.UserId == user.Id && ur.RoleId == role.Id);
            Assert.NotNull(userRole);
            Assert.Equal(user.Id, userRole.CreatedById);
        }

        [Fact]
        public async Task AssignRoleToUserAsync_AssignsRoleWithCreatedBy_WhenProvided()
        {
            using var db = new TestDbContextWrapper();
            var user = new User { FirstName = "M", LastName = "N", Email = "m@n.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var creator = new User { FirstName = "O", LastName = "P", Email = "o@p.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var app = new Application { Code = "APP12", Name = "App12" };
            var role = new Role { Code = "EDITOR", Name = "Editor", Application = app };
            db.Context.Users.AddRange(user, creator);
            db.Context.Applications.Add(app);
            db.Context.Roles.Add(role);
            db.Context.SaveChanges();
            
            var repo = new RoleRepository(db.Context);
            var result = await repo.AssignRoleToUserAsync(role.Id, user.Id, creator.Id);
            
            Assert.True(result);
            var userRole = db.Context.UserRoles.FirstOrDefault(ur => ur.UserId == user.Id && ur.RoleId == role.Id);
            Assert.NotNull(userRole);
            Assert.Equal(creator.Id, userRole.CreatedById);
        }

        [Fact]
        public async Task AssignRoleToUserAsync_ReturnsFalse_WhenRoleIdIsZero()
        {
            using var db = new TestDbContextWrapper();
            var repo = new RoleRepository(db.Context);
            
            var result = await repo.AssignRoleToUserAsync(0, 1);
            
            Assert.False(result);
            Assert.Empty(db.Context.UserRoles);
        }

        [Fact]
        public async Task AssignRoleToUserAsync_ReturnsFalse_WhenUserIdIsZero()
        {
            using var db = new TestDbContextWrapper();
            var repo = new RoleRepository(db.Context);
            
            var result = await repo.AssignRoleToUserAsync(1, 0);
            
            Assert.False(result);
            Assert.Empty(db.Context.UserRoles);
        }

        [Fact]
        public async Task AssignRoleToUserAsync_ReturnsFalse_WhenBothIdsAreZero()
        {
            using var db = new TestDbContextWrapper();
            var repo = new RoleRepository(db.Context);
            
            var result = await repo.AssignRoleToUserAsync(0, 0);
            
            Assert.False(result);
            Assert.Empty(db.Context.UserRoles);
        }

        [Fact]
        public async Task AssignRoleToUserAsync_SetsCreatedAt()
        {
            using var db = new TestDbContextWrapper();
            var user = new User { FirstName = "Q", LastName = "R", Email = "q@r.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var app = new Application { Code = "APP13", Name = "App13" };
            var role = new Role { Code = "VIEWER", Name = "Viewer", Application = app };
            db.Context.Users.Add(user);
            db.Context.Applications.Add(app);
            db.Context.Roles.Add(role);
            db.Context.SaveChanges();
            
            var beforeAssignment = DateTime.UtcNow;
            var repo = new RoleRepository(db.Context);
            var result = await repo.AssignRoleToUserAsync(role.Id, user.Id);
            var afterAssignment = DateTime.UtcNow;
            
            Assert.True(result);
            var userRole = db.Context.UserRoles.FirstOrDefault(ur => ur.UserId == user.Id && ur.RoleId == role.Id);
            Assert.NotNull(userRole);
            Assert.InRange(userRole.CreatedAt, beforeAssignment, afterAssignment);
        }

        #endregion
    }
}
