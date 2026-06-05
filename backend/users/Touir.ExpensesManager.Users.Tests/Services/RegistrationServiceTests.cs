using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Services;
using Touir.ExpensesManager.Users.Services.Contracts;
using Touir.ExpensesManager.Users.Repositories.Contracts;
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

        private static Mock<IOutboxRepository> CreateDefaultOutboxMock()
        {
            var mock = new Mock<IOutboxRepository>();
            mock.Setup(r => r.EnqueueAsync(It.IsAny<OutboxEvent>())).Returns(Task.CompletedTask);
            return mock;
        }

        private static RegistrationService CreateService(
            Mock<IUserRepository>? userRepo = null,
            Mock<IUserRoleAssignmentService>? roleAssignment = null,
            Mock<IOutboxRepository>? outboxRepo = null)
        {
            var outbox = outboxRepo ?? CreateDefaultOutboxMock();

            return new RegistrationService(
                Options.Create(new AuthenticationServiceOptions { VerifyEmailBaseUrl = "http://localhost/verify", EmailVerificationExpiryInHours = 24 }),
                userRepo?.Object ?? new Mock<IUserRepository>().Object,
                outbox.Object,
                roleAssignment?.Object ?? CreateRoleAssignmentMock().Object
            );
        }

        #region RegisterNewUserAsync Tests

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
        public async Task RegisterNewUserAsync_ResendsVerificationEmail_WhenRegistrationOngoing()
        {
            var user = new User { Id = 1, Email = "test@test.com", IsEmailValidated = false, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync(user);
            userRepo.Setup(r => r.GetUsedEmailValidationHashesAsync()).ReturnsAsync(new List<string>());
            userRepo.Setup(r => r.UpdateEmailValidationHashAsync(user.Id, It.IsAny<string>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);

            var service = CreateService(userRepo);
            var errors = await service.RegisterNewUserAsync("John", "Doe", "test@test.com", "APP1");

            Assert.Empty(errors);
            userRepo.Verify(r => r.UpdateEmailValidationHashAsync(user.Id, It.IsAny<string>(), It.IsAny<DateTime>()), Times.Once);
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
        public async Task RegisterNewUserAsync_AllowsRegistration_WhenEmailBelongsToSoftDeletedUser()
        {
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync((User?)null);
            userRepo.Setup(r => r.GetUsedEmailValidationHashesAsync()).ReturnsAsync(new List<string>());
            userRepo.Setup(r => r.CreateUserAsync(It.IsAny<User>())).ReturnsAsync((User u) => { u.Id = 2; return u; });

            var service = CreateService(userRepo);
            var errors = await service.RegisterNewUserAsync("Jane", "Doe", "test@test.com", null);

            Assert.Empty(errors);
            userRepo.Verify(r => r.CreateUserAsync(It.Is<User>(u => u.Email == "test@test.com")), Times.Once);
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
        public async Task RegisterNewUserAsync_DeletesUser_WhenOutboxEnqueueFails()
        {
            var user = new User { Id = 1, Email = "test@test.com", CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("test@test.com")).ReturnsAsync((User?)null);
            userRepo.Setup(r => r.GetUsedEmailValidationHashesAsync()).ReturnsAsync(new List<string>());
            userRepo.Setup(r => r.CreateUserAsync(It.IsAny<User>())).ReturnsAsync(user);
            userRepo.Setup(r => r.DeleteUserAsync(user)).ReturnsAsync(true);

            var outboxRepo = new Mock<IOutboxRepository>();
            outboxRepo.Setup(r => r.EnqueueAsync(It.IsAny<OutboxEvent>())).ThrowsAsync(new Exception("Outbox write failed"));

            var service = CreateService(userRepo, outboxRepo: outboxRepo);

            await Assert.ThrowsAsync<Exception>(async () =>
                await service.RegisterNewUserAsync("John", "Doe", "test@test.com", null));

            userRepo.Verify(r => r.DeleteUserAsync(user), Times.Once);
        }

        #endregion

        #region ResendVerificationEmailAsync Tests

        [Fact]
        public async Task ResendVerificationEmailAsync_ReturnsNotFound_WhenUserDoesNotExist()
        {
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("nobody@test.com")).ReturnsAsync((User?)null);

            var service = CreateService(userRepo);
            var result = await service.ResendVerificationEmailAsync("nobody@test.com", "APP1");

            Assert.Equal(ResendResult.NotFound, result);
        }

        [Fact]
        public async Task ResendVerificationEmailAsync_ReturnsNotFound_WhenEmailAlreadyValidated()
        {
            var user = new User { Id = 1, Email = "done@test.com", IsEmailValidated = true, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("done@test.com")).ReturnsAsync(user);

            var service = CreateService(userRepo);
            var result = await service.ResendVerificationEmailAsync("done@test.com", "APP1");

            Assert.Equal(ResendResult.NotFound, result);
        }

        [Fact]
        public async Task ResendVerificationEmailAsync_ReturnsSent_WhenUserIsUnverified()
        {
            var user = new User { Id = 1, Email = "pending@test.com", IsEmailValidated = false, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("pending@test.com")).ReturnsAsync(user);
            userRepo.Setup(r => r.GetUsedEmailValidationHashesAsync()).ReturnsAsync(new List<string>());
            userRepo.Setup(r => r.UpdateEmailValidationHashAsync(1, It.IsAny<string>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);

            var service = CreateService(userRepo);
            var result = await service.ResendVerificationEmailAsync("pending@test.com", "APP1");

            Assert.Equal(ResendResult.Sent, result);
        }

        [Fact]
        public async Task ResendVerificationEmailAsync_UpdatesHashWithExpiry()
        {
            var user = new User { Id = 1, Email = "pending@test.com", IsEmailValidated = false, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("pending@test.com")).ReturnsAsync(user);
            userRepo.Setup(r => r.GetUsedEmailValidationHashesAsync()).ReturnsAsync(new List<string>());
            userRepo.Setup(r => r.UpdateEmailValidationHashAsync(1, It.IsAny<string>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);

            var before = DateTime.UtcNow;
            var service = CreateService(userRepo);
            await service.ResendVerificationEmailAsync("pending@test.com", "APP1");

            userRepo.Verify(r => r.UpdateEmailValidationHashAsync(
                1,
                It.IsAny<string>(),
                It.Is<DateTime>(d => d >= before.AddHours(23) && d <= before.AddHours(25))),
                Times.Once);
        }

        [Fact]
        public async Task ResendVerificationEmailAsync_EnqueuesOutboxEvent()
        {
            var user = new User { Id = 1, Email = "pending@test.com", IsEmailValidated = false, CreatedAt = DateTime.UtcNow, LastUpdatedAt = DateTime.UtcNow };
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("pending@test.com")).ReturnsAsync(user);
            userRepo.Setup(r => r.GetUsedEmailValidationHashesAsync()).ReturnsAsync(new List<string>());
            userRepo.Setup(r => r.UpdateEmailValidationHashAsync(1, It.IsAny<string>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);

            var outboxRepo = new Mock<IOutboxRepository>();
            outboxRepo.Setup(r => r.EnqueueAsync(It.IsAny<OutboxEvent>())).Returns(Task.CompletedTask);

            var service = CreateService(userRepo, outboxRepo: outboxRepo);
            await service.ResendVerificationEmailAsync("pending@test.com", "APP1");

            outboxRepo.Verify(r => r.EnqueueAsync(It.Is<OutboxEvent>(e =>
                e.EventType == "user.email.verification.requested")), Times.Once);
        }

        [Fact]
        public async Task ResendVerificationEmailAsync_DoesNotUpdateRepo_WhenUserNotFound()
        {
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetUserByEmailAsync("nobody@test.com")).ReturnsAsync((User?)null);

            var service = CreateService(userRepo);
            await service.ResendVerificationEmailAsync("nobody@test.com", "APP1");

            userRepo.Verify(r => r.UpdateEmailValidationHashAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
        }

        #endregion

        #region ValidateEmailAsync Tests

        [Fact]
        public async Task ValidateEmailAsync_ReturnsFalse_WhenEmailFormatInvalid()
        {
            var service = CreateService();
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
            var user = new User { Id = 1, Email = "test@test.com", FirstName = "John", LastName = "Doe", IsEmailValidated = true };
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.ValidateEmailAsync(It.IsAny<string>(), "test@test.com"))
                .ReturnsAsync(user);

            var outboxRepo = new Mock<IOutboxRepository>();
            outboxRepo.Setup(r => r.EnqueueAsync(It.IsAny<OutboxEvent>())).Returns(Task.CompletedTask);

            var service = CreateService(userRepo, outboxRepo: outboxRepo);
            var result = await service.ValidateEmailAsync(Guid.NewGuid().ToString(), "test@test.com");

            Assert.True(result);
            userRepo.Verify(r => r.ValidateEmailAsync(It.IsAny<string>(), "test@test.com"), Times.Once);
            outboxRepo.Verify(r => r.EnqueueAsync(It.IsAny<OutboxEvent>()), Times.Once);
        }

        [Fact]
        public async Task ValidateEmailAsync_ReturnsFalse_WhenUserNotFound()
        {
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.ValidateEmailAsync(It.IsAny<string>(), "test@test.com"))
                .ReturnsAsync((User?)null);

            var outboxRepo = new Mock<IOutboxRepository>();

            var service = CreateService(userRepo, outboxRepo: outboxRepo);
            var result = await service.ValidateEmailAsync(Guid.NewGuid().ToString(), "test@test.com");

            Assert.False(result);
            outboxRepo.Verify(r => r.EnqueueAsync(It.IsAny<OutboxEvent>()), Times.Never);
        }

        #endregion
    }
}
