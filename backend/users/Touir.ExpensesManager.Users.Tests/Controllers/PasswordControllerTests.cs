using Touir.ExpensesManager.Users.Controllers;
using Touir.ExpensesManager.Users.Controllers.Requests;
using Touir.ExpensesManager.Users.Controllers.Responses;
using Touir.ExpensesManager.Users.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Touir.ExpensesManager.Users.Tests.Controllers
{
    public class PasswordControllerTests
    {
        private static PasswordController CreateController(IPasswordManagementService? service = null)
        {
            return new PasswordController(service ?? Mock.Of<IPasswordManagementService>());
        }

        #region ChangePassword Tests

        [Fact]
        public async Task ChangePassword_ReturnsUnauthorized_WhenServiceReturnsFalse()
        {
            var mockService = new Mock<IPasswordManagementService>();
            mockService.Setup(s => s.ChangePasswordAsync("john@doe.com", "old", "newPassword1")).ReturnsAsync(false);
            var controller = CreateController(mockService.Object);
            var request = new ChangePasswordRequest { Email = "john@doe.com", OldPassword = "old", NewPassword = "newPassword1", ConfirmPassword = "newPassword1" };
            var result = await controller.ChangePassword(request);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("SET_NEW_PASSWORD_FAILED", response.Message);
        }

        [Fact]
        public async Task ChangePassword_ReturnsOk_WhenValid()
        {
            var mockService = new Mock<IPasswordManagementService>();
            mockService.Setup(s => s.ChangePasswordAsync("john@doe.com", "old", "newPassword1")).ReturnsAsync(true);
            var controller = CreateController(mockService.Object);
            var request = new ChangePasswordRequest { Email = "john@doe.com", OldPassword = "old", NewPassword = "newPassword1", ConfirmPassword = "newPassword1" };

            var result = await controller.ChangePassword(request);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task ChangePassword_ReturnsBadRequest_WhenExceptionThrown()
        {
            var mockService = new Mock<IPasswordManagementService>();
            mockService.Setup(s => s.ChangePasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Database error"));
            var controller = CreateController(mockService.Object);
            var request = new ChangePasswordRequest { Email = "john@doe.com", OldPassword = "old", NewPassword = "newPassword1", ConfirmPassword = "newPassword1" };
            var result = await controller.ChangePassword(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(badRequestResult.Value);
            Assert.Equal("SERVER_ERROR", response.Message);
        }

        #endregion

        #region RequestPasswordReset Tests

        [Fact]
        public async Task RequestPasswordReset_ReturnsUnauthorized_WhenServiceReturnsFalse()
        {
            var mockService = new Mock<IPasswordManagementService>();
            mockService.Setup(s => s.RequestPasswordResetAsync("john@doe.com", "EXPENSES_MANAGER")).ReturnsAsync(false);
            var controller = CreateController(mockService.Object);
            var request = new RequestPasswordResetRequest { Email = "john@doe.com", AppCode = "EXPENSES_MANAGER" };
            var result = await controller.RequestPasswordReset(request);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("REQUEST_PASSWORD_RESET_FAILED", response.Message);
        }

        [Fact]
        public async Task RequestPasswordReset_ReturnsOk_WhenValid()
        {
            var mockService = new Mock<IPasswordManagementService>();
            mockService.Setup(s => s.RequestPasswordResetAsync("john@doe.com", "EXPENSES_MANAGER")).ReturnsAsync(true);
            var controller = CreateController(mockService.Object);
            var request = new RequestPasswordResetRequest { Email = "john@doe.com", AppCode = "EXPENSES_MANAGER" };

            var result = await controller.RequestPasswordReset(request);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task RequestPasswordReset_ReturnsBadRequest_WhenExceptionThrown()
        {
            var mockService = new Mock<IPasswordManagementService>();
            mockService.Setup(s => s.RequestPasswordResetAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Database error"));
            var controller = CreateController(mockService.Object);
            var request = new RequestPasswordResetRequest { Email = "john@doe.com", AppCode = "EXPENSES_MANAGER" };
            var result = await controller.RequestPasswordReset(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(badRequestResult.Value);
            Assert.Equal("SERVER_ERROR", response.Message);
        }

        #endregion

        #region ChangePasswordReset Tests

        [Fact]
        public async Task ChangePasswordReset_ReturnsUnauthorized_WhenServiceReturnsFalse()
        {
            var mockService = new Mock<IPasswordManagementService>();
            mockService.Setup(s => s.ResetPasswordAsync("john@doe.com", "hash", "newPassword1")).ReturnsAsync(false);
            var controller = CreateController(mockService.Object);
            var request = new ChangePasswordResetRequest { Email = "john@doe.com", VerificationHash = "hash", NewPassword = "newPassword1", ConfirmPassword = "newPassword1" };
            var result = await controller.ChangePasswordReset(request);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("RESET_PASSWORD_FAILED", response.Message);
        }

        [Fact]
        public async Task ChangePasswordReset_ReturnsOk_WhenValid()
        {
            var mockService = new Mock<IPasswordManagementService>();
            mockService.Setup(s => s.ResetPasswordAsync("john@doe.com", "hash", "newPassword1")).ReturnsAsync(true);
            var controller = CreateController(mockService.Object);
            var request = new ChangePasswordResetRequest { Email = "john@doe.com", VerificationHash = "hash", NewPassword = "newPassword1", ConfirmPassword = "newPassword1" };

            var result = await controller.ChangePasswordReset(request);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task ChangePasswordReset_ReturnsBadRequest_WhenExceptionThrown()
        {
            var mockService = new Mock<IPasswordManagementService>();
            mockService.Setup(s => s.ResetPasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Database error"));
            var controller = CreateController(mockService.Object);
            var request = new ChangePasswordResetRequest { Email = "john@doe.com", VerificationHash = "hash", NewPassword = "newPassword1", ConfirmPassword = "newPassword1" };
            var result = await controller.ChangePasswordReset(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(badRequestResult.Value);
            Assert.Equal("SERVER_ERROR", response.Message);
        }

        #endregion
    }
}
