using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Services;
using Touir.ExpensesManager.Users.Services.Contracts;
using Touir.ExpensesManager.Users.Repositories.Contracts;
using Touir.ExpensesManager.Users.Infrastructure.Contracts;
using Touir.ExpensesManager.Users.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Moq;

namespace Touir.ExpensesManager.Users.Tests.Services
{
    public class RegistrationServiceTests
    {
        private static Mock<IUserRoleAssignmentService> CreateRoleAssignmentMock()
        {
            var mock = new Mock<IUserRoleAssignmentService>();
            mock.Setup(s => s.TryAssignDefaultRoleAsync(It.IsAny<string?>(), It.IsAny<User?>())).Returns(Task.CompletedTask);
            return mock;
        }

        private static Mock<IEmailHelper> CreateEmailHelperMock()
        {
            var mock = new Mock<IEmailHelper>();
            mock.Setup(e => e.VerifyEmail(It.IsAny<string>())).Returns(true);
            mock.Setup(e => e.GetEmailTemplate(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).Returns("<html></html>");
            mock.Setup(e => e.SendEmail(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<ICollection<string>>())).Returns(true);
            return mock;
        }

        private static RegistrationService CreateService(
            Mock<IUserRepository>? userRepo = null,
            Mock<IUserRoleAssignmentService>? roleAssignment = null,
            Mock<IEmailHelper>? emailHelper = null)
        {
            return new RegistrationService(
                Options.Create(new AuthenticationServiceOptions { VerifyEmailBaseUrl = "http://localhost/verify" }),
                emailHelper?.Object ?? CreateEmailHelperMock().Object,
                userRepo?.Object ?? new Mock<IUserRepository>().Object,
                roleAssignment?.Object ?? CreateRoleAssignmentMock().Object
            );
        }

        #region RegisterNewUserAsync Tests

        [Fact]
        public async Task RegisterNewUserAsync_ReturnsError_WhenEmailFormatIsInvalid()
        {
            var emailHelper = new Mock<IEmailHelper>();
            emailHelper.Setup(e => e.VerifyEmail("invalidemail")).Returns(false);

            var service = CreateService(emailHelper: emailHelper);
            var errors = await service.RegisterNewUserAsync("John", "Doe", "invalidemail", "APP1");

            Assert.Single(errors);
            Assert.Contains("email format is invalid", errors);
        }

        [Fact]
        public async Task RegisterNewUserAsync_ReturnsError_WhenEmailAlreadyValidated()
        {
            var user = new User { Id = 1, Email = "test@test.com", IsEmailValidated = true, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync(user);

            var service = CreateService(userRepo);
            var errors = await service.RegisterNewUserAsync("John", "Doe", "test@test.com", "APP1");

            Assert.Single(errors);
            Assert.Contains("email is already used", errors);
        }

        [Fact]
        public async Task RegisterNewUserAsync_ReturnsError_WhenRegistrationOngoing()
        {
            var user = new User { Id = 1, Email = "test@test.com", IsEmailValidated = false, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync(user);

            var service = CreateService(userRepo);
            var errors = await service.RegisterNewUserAsync("John", "Doe", "test@test.com", "APP1");

            Assert.Single(errors);
            Assert.Contains("there's already a registration process ongoing, please check you mailbox to validate the email", errors);
        }

        [Fact]
        public async Task RegisterNewUserAsync_CreatesUser_WhenValid()
        {
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync((User?)null);
            userRepo.Setup(r => r.GetUsedEmailValidationHashesAsync()).ReturnsAsync(new List<string>());
            userRepo.Setup(r => r.CreateUserAsync(It.IsAny<User>())).ReturnsAsync((User u) => { u.Id = 1; return u; });

            var service = CreateService(userRepo);
            var errors = await service.RegisterNewUserAsync("John", "Doe", "test@test.com", null);

            Assert.Empty(errors);
            userRepo.Verify(r => r.CreateUserAsync(It.Is<User>(u => u.Email == "test@test.com" && u.FirstName == "John")), Times.Once);
        }

        [Fact]
        public async Task RegisterNewUserAsync_AssignsDefaultRole_WhenApplicationCodeProvided()
        {
            var user = new User { Id = 1, Email = "test@test.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };

            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync((User?)null);
            userRepo.Setup(r => r.GetUsedEmailValidationHashesAsync()).ReturnsAsync(new List<string>());
            userRepo.Setup(r => r.CreateUserAsync(It.IsAny<User>())).ReturnsAsync(user);

            var roleAssignment = new Mock<IUserRoleAssignmentService>();
            roleAssignment.Setup(s => s.TryAssignDefaultRoleAsync("APP1", user)).Returns(Task.CompletedTask);

            var service = CreateService(userRepo, roleAssignment: roleAssignment);
            var errors = await service.RegisterNewUserAsync("John", "Doe", "test@test.com", "APP1");

            Assert.Empty(errors);
            roleAssignment.Verify(s => s.TryAssignDefaultRoleAsync("APP1", user), Times.Once);
        }

        [Fact]
        public async Task RegisterNewUserAsync_DeletesUser_WhenEmailSendFails()
        {
            var user = new User { Id = 1, Email = "test@test.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync((User?)null);
            userRepo.Setup(r => r.GetUsedEmailValidationHashesAsync()).ReturnsAsync(new List<string>());
            userRepo.Setup(r => r.CreateUserAsync(It.IsAny<User>())).ReturnsAsync(user);
            userRepo.Setup(r => r.DeleteUserAsync(user)).ReturnsAsync(true);

            var emailHelper = new Mock<IEmailHelper>();
            emailHelper.Setup(e => e.VerifyEmail(It.IsAny<string>())).Returns(true);
            emailHelper.Setup(e => e.GetEmailTemplate(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).Returns("<html></html>");
            emailHelper.Setup(e => e.SendEmail(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<ICollection<string>>()))
                .Throws(new Exception("Email send failed"));

            var service = CreateService(userRepo, emailHelper: emailHelper);

            await Assert.ThrowsAsync<Exception>(async () =>
                await service.RegisterNewUserAsync("John", "Doe", "test@test.com", null));

            userRepo.Verify(r => r.DeleteUserAsync(user), Times.Once);
        }

        #endregion

        #region ValidateEmailAsync Tests

        [Fact]
        public async Task ValidateEmailAsync_ReturnsFalse_WhenEmailFormatInvalid()
        {
            var emailHelper = new Mock<IEmailHelper>();
            emailHelper.Setup(e => e.VerifyEmail("invalidemail")).Returns(false);

            var service = CreateService(emailHelper: emailHelper);
            var result = await service.ValidateEmailAsync("hash", "invalidemail");

            Assert.False(result);
        }

        [Fact]
        public async Task ValidateEmailAsync_ReturnsFalse_WhenHashIsNotGuid()
        {
            var service = CreateService();
            var result = await service.ValidateEmailAsync("not-a-guid", "test@test.com");

            Assert.False(result);
        }

        [Fact]
        public async Task ValidateEmailAsync_ReturnsTrue_WhenValid()
        {
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.ValidateEmail(It.IsAny<string>(), "test@test.com")).ReturnsAsync(true);

            var service = CreateService(userRepo);
            var result = await service.ValidateEmailAsync(Guid.NewGuid().ToString(), "test@test.com");

            Assert.True(result);
        }

        #endregion
    }
}
