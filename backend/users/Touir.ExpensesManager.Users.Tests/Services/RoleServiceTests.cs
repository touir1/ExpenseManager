using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Services;
using Touir.ExpensesManager.Users.Repositories;
using Touir.ExpensesManager.Users.Tests.Repositories.Helpers;
using Touir.ExpensesManager.Users.Repositories.Contracts;
using Moq;

namespace Touir.ExpensesManager.Users.Tests.Services
{
    public class RoleServiceTests
    {
        [Fact]
        public async Task GetUserRolesByApplicationCodeAsync_ReturnsRoles()
        {
            var mockRepo = new Mock<IRoleRepository>();
            mockRepo.Setup(r => r.GetUserRolesByApplicationCodeAsync("APP1", 1))
                .ReturnsAsync(new List<Role> { new Role { Id = 1, Code = "ADMIN", Name = "Admin" } });
            var service = new RoleService(mockRepo.Object);
            var roles = await service.GetUserRolesByApplicationCodeAsync("APP1", 1);
            Assert.Single(roles);
            Assert.Equal("ADMIN", roles.First().Code);
        }

        [Fact]
        public async Task GetUserRolesByApplicationCodeAsync_ReturnsNullForNullCode()
        {
            var mockRepo = new Mock<IRoleRepository>();
            var service = new RoleService(mockRepo.Object);
            var roles = await service.GetUserRolesByApplicationCodeAsync(null, 1);
            Assert.Null(roles);
        }
    }
}
