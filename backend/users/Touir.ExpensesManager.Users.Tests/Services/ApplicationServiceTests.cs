using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Services;
using Touir.ExpensesManager.Users.Repositories.Contracts;
using Moq;

namespace Touir.ExpensesManager.Users.Tests.Services
{
    public class ApplicationServiceTests
    {
        [Fact]
        public async Task GetApplicationByCodeAsync_ReturnsCorrectApplication()
        {
            var mockRepo = new Mock<IApplicationRepository>();
            mockRepo.Setup(r => r.GetApplicationByCodeAsync("APP1"))
                .ReturnsAsync(new Application { Id = 1, Code = "APP1", Name = "App1" });
            
            var service = new ApplicationService(mockRepo.Object);
            var result = await service.GetApplicationByCodeAsync("APP1");
            
            Assert.NotNull(result);
            Assert.Equal("APP1", result.Code);
        }

        [Fact]
        public async Task GetApplicationByCodeAsync_ReturnsNull_WhenNotFound()
        {
            var mockRepo = new Mock<IApplicationRepository>();
            mockRepo.Setup(r => r.GetApplicationByCodeAsync("NON_EXISTENT"))
                .ReturnsAsync((Application)null);
            
            var service = new ApplicationService(mockRepo.Object);
            var result = await service.GetApplicationByCodeAsync("NON_EXISTENT");
            
            Assert.Null(result);
        }
    }
}
