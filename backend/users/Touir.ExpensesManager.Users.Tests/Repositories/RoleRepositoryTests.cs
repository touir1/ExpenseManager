using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Repositories;
using Touir.ExpensesManager.Users.Tests.Repositories.Helpers;

namespace Touir.ExpensesManager.Users.Tests.Repositories
{
    public class RoleRepositoryTests
    {
        [Fact]
        public async Task GetUserRolesByApplicationCodeAsync_ReturnsRoles()
        {
            using var db = new TestDbContextWrapper();
            var app = new Application { Id = 1, Code = "APP1", Name = "App1" };
            var user = new User { Id = 1, FirstName = "A", LastName = "B", Email = "a@b.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var role = new Role { Id = 1, Code = "ADMIN", Name = "Admin", ApplicationId = 1, Application = app };
            var userRole = new UserRole { UserId = 1, RoleId = 1, User = user, Role = role, CreatedAt = DateTime.UtcNow };
            db.Context.Applications.Add(app);
            db.Context.Users.Add(user);
            db.Context.Roles.Add(role);
            db.Context.UserRoles.Add(userRole);
            db.Context.SaveChanges();
            
            var repo = new RoleRepository(db.Context);
            var roles = await repo.GetUserRolesByApplicationCodeAsync("APP1", 1);
            
            Assert.Single(roles);
            Assert.Equal("ADMIN", roles.First().Code);
        }

        [Fact]
        public async Task GetDefaultRoleByApplicationIdAsync_ReturnsDefaultRole()
        {
            using var db = new TestDbContextWrapper();
            var app = new Application { Id = 2, Code = "APP2", Name = "App2" };
            var role = new Role { Id = 2, Code = "USER", Name = "User", ApplicationId = 2, Application = app, IsDefault = true };
            db.Context.Applications.Add(app);
            db.Context.Roles.Add(role);
            db.Context.SaveChanges();
            
            var repo = new RoleRepository(db.Context);
            var result = await repo.GetDefaultRoleByApplicationIdAsync(2);
            
            Assert.NotNull(result);
            Assert.True(result.IsDefault);
            Assert.Equal("USER", result.Code);
        }

        [Fact]
        public async Task AssignRoleToUserAsync_AssignsRole()
        {
            using var db = new TestDbContextWrapper();
            var user = new User { Id = 3, FirstName = "C", LastName = "D", Email = "c@d.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var app = new Application { Id = 3, Code = "APP3", Name = "App3" };
            var role = new Role { Id = 3, Code = "MOD", Name = "Moderator", ApplicationId = 3, Application = app };
            db.Context.Users.Add(user);
            db.Context.Applications.Add(app);
            db.Context.Roles.Add(role);
            db.Context.SaveChanges();
            
            var repo = new RoleRepository(db.Context);
            var result = await repo.AssignRoleToUserAsync(3, 3);
            
            Assert.True(result);
            Assert.Single(db.Context.UserRoles.Where(ur => ur.UserId == 3 && ur.RoleId == 3));
        }
    }
}
