using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Services;
using Touir.ExpensesManager.Users.Controllers.EO;
using Touir.ExpensesManager.Users.Repositories.Contracts;
using Moq;

namespace Touir.ExpensesManager.Users.Tests.Services
{
    public class RoleServiceTests
    {
        #region GetUserRolesByApplicationCodeAsync Tests

        [Fact]
        public async Task GetUserRolesByApplicationCodeAsync_ReturnsEmpty_WhenApplicationCodeIsNull()
        {
            var mockRepo = new Mock<IRoleRepository>();
            var service = new RoleService(mockRepo.Object);

            var result = await service.GetUserRolesByApplicationCodeAsync(null!, 1);

            Assert.NotNull(result);
            Assert.Empty(result);
            mockRepo.Verify(r => r.GetUserRolesByApplicationCodeAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetUserRolesByApplicationCodeAsync_ReturnsRoleEos_WhenRolesExist()
        {
            var app = new Application { Id = 1, Code = "APP1", Name = "App1", Description = "Application 1", UrlPath = "http://app1" };
            var role = new Role 
            { 
                Id = 1, 
                Code = "ADMIN", 
                Name = "Admin", 
                Description = "Administrator",
                Application = app,
                ApplicationId = 1
            };

            var mockRepo = new Mock<IRoleRepository>();
            mockRepo.Setup(r => r.GetUserRolesByApplicationCodeAsync("APP1", 1))
                .ReturnsAsync(new List<Role> { role });
            
            var service = new RoleService(mockRepo.Object);
            var result = await service.GetUserRolesByApplicationCodeAsync("APP1", 1);
            
            Assert.NotNull(result);
            Assert.Single(result);
            var roleEo = result.First();
            Assert.Equal(1, roleEo.Id);
            Assert.Equal("ADMIN", roleEo.Code);
            Assert.Equal("Admin", roleEo.Name);
            Assert.Equal("Administrator", roleEo.Description);
            Assert.NotNull(roleEo.Application);
            Assert.Equal(1, roleEo.Application.Id);
            Assert.Equal("APP1", roleEo.Application.Code);
            Assert.Equal("App1", roleEo.Application.Name);
            Assert.Equal("Application 1", roleEo.Application.Description);
            Assert.Equal("http://app1", roleEo.Application.UrlPath);
        }

        [Fact]
        public async Task GetUserRolesByApplicationCodeAsync_ReturnsMultipleRoleEos_WhenMultipleRolesExist()
        {
            var app = new Application { Id = 1, Code = "APP1", Name = "App1", Description = "Application 1", UrlPath = "http://app1" };
            var role1 = new Role 
            { 
                Id = 1, 
                Code = "ADMIN", 
                Name = "Admin", 
                Description = "Administrator",
                Application = app,
                ApplicationId = 1
            };
            var role2 = new Role 
            { 
                Id = 2, 
                Code = "USER", 
                Name = "User", 
                Description = "Regular User",
                Application = app,
                ApplicationId = 1
            };

            var mockRepo = new Mock<IRoleRepository>();
            mockRepo.Setup(r => r.GetUserRolesByApplicationCodeAsync("APP1", 1))
                .ReturnsAsync(new List<Role> { role1, role2 });
            
            var service = new RoleService(mockRepo.Object);
            var result = await service.GetUserRolesByApplicationCodeAsync("APP1", 1);
            
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, r => r.Code == "ADMIN");
            Assert.Contains(result, r => r.Code == "USER");
        }

        [Fact]
        public async Task GetUserRolesByApplicationCodeAsync_ReturnsEmptyList_WhenNoRolesExist()
        {
            var mockRepo = new Mock<IRoleRepository>();
            mockRepo.Setup(r => r.GetUserRolesByApplicationCodeAsync("APP1", 1))
                .ReturnsAsync(new List<Role>());
            
            var service = new RoleService(mockRepo.Object);
            var result = await service.GetUserRolesByApplicationCodeAsync("APP1", 1);
            
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUserRolesByApplicationCodeAsync_ReturnsEmpty_WhenRepositoryReturnsNull()
        {
            var mockRepo = new Mock<IRoleRepository>();
            mockRepo.Setup(r => r.GetUserRolesByApplicationCodeAsync("APP1", 1))
                .ReturnsAsync((IEnumerable<Role>?)null);

            var service = new RoleService(mockRepo.Object);
            var result = await service.GetUserRolesByApplicationCodeAsync("APP1", 1);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUserRolesByApplicationCodeAsync_MapsRoleWithNullApplication()
        {
            var role = new Role 
            { 
                Id = 1, 
                Code = "ADMIN", 
                Name = "Admin", 
                Description = "Administrator",
                Application = null!,
                ApplicationId = 1
            };

            var mockRepo = new Mock<IRoleRepository>();
            mockRepo.Setup(r => r.GetUserRolesByApplicationCodeAsync("APP1", 1))
                .ReturnsAsync(new List<Role> { role });
            
            var service = new RoleService(mockRepo.Object);
            var result = await service.GetUserRolesByApplicationCodeAsync("APP1", 1);
            
            Assert.NotNull(result);
            Assert.Single(result);
            var roleEo = result.First();
            Assert.Equal(1, roleEo.Id);
            Assert.Equal("ADMIN", roleEo.Code);
            Assert.Equal("Admin", roleEo.Name);
            Assert.Null(roleEo.Application);
        }

        [Fact]
        public async Task GetUserRolesByApplicationCodeAsync_MapsRoleWithNullDescription()
        {
            var app = new Application { Id = 1, Code = "APP1", Name = "App1" };
            var role = new Role 
            { 
                Id = 1, 
                Code = "ADMIN", 
                Name = "Admin", 
                Description = null,
                Application = app,
                ApplicationId = 1
            };

            var mockRepo = new Mock<IRoleRepository>();
            mockRepo.Setup(r => r.GetUserRolesByApplicationCodeAsync("APP1", 1))
                .ReturnsAsync(new List<Role> { role });
            
            var service = new RoleService(mockRepo.Object);
            var result = await service.GetUserRolesByApplicationCodeAsync("APP1", 1);
            
            Assert.NotNull(result);
            Assert.Single(result);
            var roleEo = result.First();
            Assert.Null(roleEo.Description);
        }

        [Fact]
        public async Task GetUserRolesByApplicationCodeAsync_CallsRepositoryWithCorrectParameters()
        {
            var mockRepo = new Mock<IRoleRepository>();
            mockRepo.Setup(r => r.GetUserRolesByApplicationCodeAsync("APP1", 123))
                .ReturnsAsync(new List<Role>());
            
            var service = new RoleService(mockRepo.Object);
            await service.GetUserRolesByApplicationCodeAsync("APP1", 123);
            
            mockRepo.Verify(r => r.GetUserRolesByApplicationCodeAsync("APP1", 123), Times.Once);
        }

        [Fact]
        public async Task GetUserRolesByApplicationCodeAsync_MapsApplicationWithNullOptionalFields()
        {
            var app = new Application 
            { 
                Id = 1, 
                Code = "APP1", 
                Name = "App1",
                Description = null,
                UrlPath = null
            };
            var role = new Role 
            { 
                Id = 1, 
                Code = "ADMIN", 
                Name = "Admin",
                Application = app,
                ApplicationId = 1
            };

            var mockRepo = new Mock<IRoleRepository>();
            mockRepo.Setup(r => r.GetUserRolesByApplicationCodeAsync("APP1", 1))
                .ReturnsAsync(new List<Role> { role });
            
            var service = new RoleService(mockRepo.Object);
            var result = await service.GetUserRolesByApplicationCodeAsync("APP1", 1);
            
            Assert.NotNull(result);
            var roleEo = result.First();
            Assert.NotNull(roleEo.Application);
            Assert.Equal("APP1", roleEo.Application.Code);
            Assert.Equal("App1", roleEo.Application.Name);
            Assert.Null(roleEo.Application.Description);
            Assert.Null(roleEo.Application.UrlPath);
        }

        #endregion
    }
}
