using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Services;
using Touir.ExpensesManager.Users.Repositories.Contracts;
using Moq;

namespace Touir.ExpensesManager.Users.Tests.Services
{
    public class UserRoleAssignmentServiceTests
    {
        private static UserRoleAssignmentService CreateService(
            Mock<IApplicationRepository>? appRepo = null,
            Mock<IRoleRepository>? roleRepo = null)
            => new(
                appRepo?.Object ?? new Mock<IApplicationRepository>().Object,
                roleRepo?.Object ?? new Mock<IRoleRepository>().Object);

        #region TryAssignDefaultRoleAsync Tests

        [Fact]
        public async Task TryAssignDefaultRoleAsync_DoesNothing_WhenApplicationCodeIsNull()
        {
            var appRepo = new Mock<IApplicationRepository>();
            var roleRepo = new Mock<IRoleRepository>();
            var service = CreateService(appRepo, roleRepo);
            var user = new User { Id = 1, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };

            await service.TryAssignDefaultRoleAsync(null, user);

            appRepo.Verify(r => r.GetApplicationByCodeAsync(It.IsAny<string>()), Times.Never);
            roleRepo.Verify(r => r.GetDefaultRoleByApplicationIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task TryAssignDefaultRoleAsync_DoesNothing_WhenUserIsNull()
        {
            var appRepo = new Mock<IApplicationRepository>();
            var roleRepo = new Mock<IRoleRepository>();
            var service = CreateService(appRepo, roleRepo);

            await service.TryAssignDefaultRoleAsync("APP1", null);

            appRepo.Verify(r => r.GetApplicationByCodeAsync(It.IsAny<string>()), Times.Never);
            roleRepo.Verify(r => r.GetDefaultRoleByApplicationIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task TryAssignDefaultRoleAsync_DoesNothing_WhenApplicationNotFound()
        {
            var appRepo = new Mock<IApplicationRepository>();
            appRepo.Setup(r => r.GetApplicationByCodeAsync("APP1")).ReturnsAsync((Application?)null);

            var roleRepo = new Mock<IRoleRepository>();
            var service = CreateService(appRepo, roleRepo);
            var user = new User { Id = 1, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };

            await service.TryAssignDefaultRoleAsync("APP1", user);

            roleRepo.Verify(r => r.GetDefaultRoleByApplicationIdAsync(It.IsAny<int>()), Times.Never);
            roleRepo.Verify(r => r.AssignRoleToUserAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task TryAssignDefaultRoleAsync_DoesNothing_WhenNoDefaultRoleExists()
        {
            var app = new Application { Id = 10, Code = "APP1", Name = "App1" };

            var appRepo = new Mock<IApplicationRepository>();
            appRepo.Setup(r => r.GetApplicationByCodeAsync("APP1")).ReturnsAsync(app);

            var roleRepo = new Mock<IRoleRepository>();
            roleRepo.Setup(r => r.GetDefaultRoleByApplicationIdAsync(10)).ReturnsAsync((Role?)null);

            var service = CreateService(appRepo, roleRepo);
            var user = new User { Id = 1, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };

            await service.TryAssignDefaultRoleAsync("APP1", user);

            roleRepo.Verify(r => r.AssignRoleToUserAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task TryAssignDefaultRoleAsync_AssignsRole_WhenApplicationAndDefaultRoleExist()
        {
            var app = new Application { Id = 10, Code = "APP1", Name = "App1" };
            var role = new Role { Id = 5, Code = "USER", Name = "User", ApplicationId = 10, Application = app };
            var user = new User { Id = 1, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };

            var appRepo = new Mock<IApplicationRepository>();
            appRepo.Setup(r => r.GetApplicationByCodeAsync("APP1")).ReturnsAsync(app);

            var roleRepo = new Mock<IRoleRepository>();
            roleRepo.Setup(r => r.GetDefaultRoleByApplicationIdAsync(10)).ReturnsAsync(role);
            roleRepo.Setup(r => r.AssignRoleToUserAsync(5, 1, null)).ReturnsAsync(true);

            var service = CreateService(appRepo, roleRepo);

            await service.TryAssignDefaultRoleAsync("APP1", user);

            roleRepo.Verify(r => r.AssignRoleToUserAsync(5, 1, null), Times.Once);
        }

        #endregion
    }
}
