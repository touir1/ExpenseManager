using Moq;
using Touir.ExpensesManager.Users.Controllers.DTO;
using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Repositories.Contracts;
using Touir.ExpensesManager.Users.Services;

namespace Touir.ExpensesManager.Users.Tests.Services
{
    public class AdminUserServiceTests
    {
        private static AdminUserService CreateService(IUserRepository? userRepo = null, IRoleRepository? roleRepo = null)
            => new(userRepo ?? Mock.Of<IUserRepository>(), roleRepo ?? Mock.Of<IRoleRepository>());

        private static User MakeUser(int id, string email, bool isDisabled = false)
        {
            return new User
            {
                Id = id,
                Email = email,
                FirstName = "First",
                LastName = "Last",
                IsDisabled = isDisabled,
                IsDeleted = false,
                IsEmailValidated = true,
                CreatedAt = DateTime.UtcNow,
                UserRoles = []
            };
        }

        [Fact]
        public async Task GetUsersPagedAsync_ReturnsMappedDtos()
        {
            var users = new List<User> { MakeUser(1, "a@b.com"), MakeUser(2, "c@d.com") };
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetPagedAsync(null, 1, 20)).ReturnsAsync((users, 2));

            var (dtos, total) = await CreateService(userRepo.Object).GetUsersPagedAsync(null, 1, 20);

            Assert.Equal(2, total);
            Assert.Equal(2, dtos.Count());
        }

        [Fact]
        public async Task GetUsersPagedAsync_FiltersUsersCorrectly()
        {
            var users = new List<User> { MakeUser(1, "match@test.com") };
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetPagedAsync("match", 1, 10)).ReturnsAsync((users, 1));

            var (dtos, total) = await CreateService(userRepo.Object).GetUsersPagedAsync("match", 1, 10);

            Assert.Equal(1, total);
            Assert.Equal("match@test.com", dtos.First().Email);
        }

        [Fact]
        public async Task DisableUserAsync_ReturnsTrue_WhenUserExists()
        {
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.DisableAsync(1)).ReturnsAsync(true);

            var result = await CreateService(userRepo.Object).DisableUserAsync(1);

            Assert.True(result);
        }

        [Fact]
        public async Task DisableUserAsync_ReturnsFalse_WhenUserNotFound()
        {
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.DisableAsync(99)).ReturnsAsync(false);

            var result = await CreateService(userRepo.Object).DisableUserAsync(99);

            Assert.False(result);
        }

        [Fact]
        public async Task EnableUserAsync_ReturnsTrue_WhenUserExists()
        {
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.EnableAsync(1)).ReturnsAsync(true);

            var result = await CreateService(userRepo.Object).EnableUserAsync(1);

            Assert.True(result);
        }

        [Fact]
        public async Task GetAllRolesAsync_ReturnsMappedRoleDtos()
        {
            var app = new Application { Id = 1, Code = "APP", Name = "App" };
            var roles = new List<Role>
            {
                new() { Id = 1, Code = "ADMIN", Name = "Administrator", ApplicationId = 1, Application = app },
                new() { Id = 2, Code = "USER", Name = "User", ApplicationId = 1, Application = app }
            };
            var roleRepo = new Mock<IRoleRepository>();
            roleRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(roles);

            var result = (await CreateService(roleRepo: roleRepo.Object).GetAllRolesAsync()).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal("ADMIN", result[0].Code);
        }

        [Fact]
        public async Task SetUserRolesAsync_RemovesThenAssignsRoles()
        {
            var roleRepo = new Mock<IRoleRepository>();
            roleRepo.Setup(r => r.RemoveUserRolesAsync(5)).Returns(Task.CompletedTask);
            roleRepo.Setup(r => r.AssignRoleToUserAsync(It.IsAny<int>(), 5, 1)).ReturnsAsync(true);

            await CreateService(roleRepo: roleRepo.Object).SetUserRolesAsync(5, [1, 2], 1);

            roleRepo.Verify(r => r.RemoveUserRolesAsync(5), Times.Once);
            roleRepo.Verify(r => r.AssignRoleToUserAsync(It.IsAny<int>(), 5, 1), Times.Exactly(2));
        }

        [Fact]
        public async Task SetUserRolesAsync_WithEmptyRoles_OnlyRemoves()
        {
            var roleRepo = new Mock<IRoleRepository>();
            roleRepo.Setup(r => r.RemoveUserRolesAsync(5)).Returns(Task.CompletedTask);

            await CreateService(roleRepo: roleRepo.Object).SetUserRolesAsync(5, [], 1);

            roleRepo.Verify(r => r.RemoveUserRolesAsync(5), Times.Once);
            roleRepo.Verify(r => r.AssignRoleToUserAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }
    }
}
