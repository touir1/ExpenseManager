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
        public async Task ChangePassword_ReturnsUnauthorized_WhenRequestIsNull()
        {
            var controller = CreateController();
            var result = await controller.ChangePassword(null!);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task ChangePassword_ReturnsUnauthorized_WhenEmailIsEmpty()
        {
            var controller = CreateController();
            var request = new ChangePasswordRequest { Email = "", OldPassword = "old", NewPassword = "new", ConfirmPassword = "new" };
            var result = await controller.ChangePassword(request);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task ChangePassword_ReturnsUnauthorized_WhenOldPasswordIsEmpty()
        {
            var controller = CreateController();
            var request = new ChangePasswordRequest { Email = "john@doe.com", OldPassword = "", NewPassword = "new", ConfirmPassword = "new" };
            var result = await controller.ChangePassword(request);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task ChangePassword_ReturnsUnauthorized_WhenNewPasswordIsEmpty()
        {
            var controller = CreateController();
            var request = new ChangePasswordRequest { Email = "john@doe.com", OldPassword = "old", NewPassword = "", ConfirmPassword = "new" };
            var result = await controller.ChangePassword(request);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task ChangePassword_ReturnsUnauthorized_WhenConfirmPasswordIsEmpty()
        {
            var controller = CreateController();
            var request = new ChangePasswordRequest { Email = "john@doe.com", OldPassword = "old", NewPassword = "new", ConfirmPassword = "" };
            var result = await controller.ChangePassword(request);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task ChangePassword_ReturnsUnauthorized_WhenPasswordsDoNotMatch()
        {
            var controller = CreateController();
            var request = new ChangePasswordRequest { Email = "john@doe.com", OldPassword = "old", NewPassword = "new", ConfirmPassword = "different" };
            var result = await controller.ChangePassword(request);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("NOT_MATCHING_CONFIRM_PASSWORD", response.Message);
        }

        [Fact]
        public async Task ChangePassword_ReturnsUnauthorized_WhenServiceReturnsFalse()
        {
            var mockService = new Mock<IPasswordManagementService>();
            mockService.Setup(s => s.ChangePasswordAsync("john@doe.com", "old", "new")).ReturnsAsync(false);
            var controller = CreateController(mockService.Object);
            var request = new ChangePasswordRequest { Email = "john@doe.com", OldPassword = "old", NewPassword = "new", ConfirmPassword = "new" };
            var result = await controller.ChangePassword(request);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("SET_NEW_PASSWORD_FAILED", response.Message);
        }

        [Fact]
        public async Task ChangePassword_ReturnsOk_WhenValid()
        {
            var mockService = new Mock<IPasswordManagementService>();
            mockService.Setup(s => s.ChangePasswordAsync("john@doe.com", "old", "new")).ReturnsAsync(true);
            var controller = CreateController(mockService.Object);
            var request = new ChangePasswordRequest { Email = "john@doe.com", OldPassword = "old", NewPassword = "new", ConfirmPassword = "new" };

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
            var request = new ChangePasswordRequest { Email = "john@doe.com", OldPassword = "old", NewPassword = "new", ConfirmPassword = "new" };
            var result = await controller.ChangePassword(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(badRequestResult.Value);
            Assert.Equal("SERVER_ERROR", response.Message);
        }

        #endregion

        #region RequestPasswordReset Tests

        [Fact]
        public async Task RequestPasswordReset_ReturnsUnauthorized_WhenRequestIsNull()
        {
            var controller = CreateController();
            var result = await controller.RequestPasswordReset(null!);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task RequestPasswordReset_ReturnsUnauthorized_WhenEmailIsEmpty()
        {
            var controller = CreateController();
            var request = new RequestPasswordResetRequest { Email = "", AppCode = "EXPENSES_MANAGER" };
            var result = await controller.RequestPasswordReset(request);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task RequestPasswordReset_ReturnsUnauthorized_WhenAppCodeIsEmpty()
        {
            var controller = CreateController();
            var request = new RequestPasswordResetRequest { Email = "john@doe.com", AppCode = "" };
            var result = await controller.RequestPasswordReset(request);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

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
        public async Task ChangePasswordReset_ReturnsUnauthorized_WhenRequestIsNull()
        {
            var controller = CreateController();
            var result = await controller.ChangePasswordReset(null!);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task ChangePasswordReset_ReturnsUnauthorized_WhenEmailIsEmpty()
        {
            var controller = CreateController();
            var request = new ChangePasswordResetRequest { Email = "", VerificationHash = "hash", NewPassword = "new", ConfirmPassword = "new" };
            var result = await controller.ChangePasswordReset(request);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task ChangePasswordReset_ReturnsUnauthorized_WhenVerificationHashIsEmpty()
        {
            var controller = CreateController();
            var request = new ChangePasswordResetRequest { Email = "john@doe.com", VerificationHash = "", NewPassword = "new", ConfirmPassword = "new" };
            var result = await controller.ChangePasswordReset(request);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task ChangePasswordReset_ReturnsUnauthorized_WhenNewPasswordIsEmpty()
        {
            var controller = CreateController();
            var request = new ChangePasswordResetRequest { Email = "john@doe.com", VerificationHash = "hash", NewPassword = "", ConfirmPassword = "new" };
            var result = await controller.ChangePasswordReset(request);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task ChangePasswordReset_ReturnsUnauthorized_WhenConfirmPasswordIsEmpty()
        {
            var controller = CreateController();
            var request = new ChangePasswordResetRequest { Email = "john@doe.com", VerificationHash = "hash", NewPassword = "new", ConfirmPassword = "" };
            var result = await controller.ChangePasswordReset(request);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task ChangePasswordReset_ReturnsUnauthorized_WhenPasswordsDoNotMatch()
        {
            var controller = CreateController();
            var request = new ChangePasswordResetRequest { Email = "john@doe.com", VerificationHash = "hash", NewPassword = "new", ConfirmPassword = "different" };
            var result = await controller.ChangePasswordReset(request);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("NOT_MATCHING_CONFIRM_PASSWORD", response.Message);
        }

        [Fact]
        public async Task ChangePasswordReset_ReturnsUnauthorized_WhenServiceReturnsFalse()
        {
            var mockService = new Mock<IPasswordManagementService>();
            mockService.Setup(s => s.ResetPasswordAsync("john@doe.com", "hash", "new")).ReturnsAsync(false);
            var controller = CreateController(mockService.Object);
            var request = new ChangePasswordResetRequest { Email = "john@doe.com", VerificationHash = "hash", NewPassword = "new", ConfirmPassword = "new" };
            var result = await controller.ChangePasswordReset(request);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("RESET_PASSWORD_FAILED", response.Message);
        }

        [Fact]
        public async Task ChangePasswordReset_ReturnsOk_WhenValid()
        {
            var mockService = new Mock<IPasswordManagementService>();
            mockService.Setup(s => s.ResetPasswordAsync("john@doe.com", "hash", "new")).ReturnsAsync(true);
            var controller = CreateController(mockService.Object);
            var request = new ChangePasswordResetRequest { Email = "john@doe.com", VerificationHash = "hash", NewPassword = "new", ConfirmPassword = "new" };

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
            var request = new ChangePasswordResetRequest { Email = "john@doe.com", VerificationHash = "hash", NewPassword = "new", ConfirmPassword = "new" };
            var result = await controller.ChangePasswordReset(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(badRequestResult.Value);
            Assert.Equal("SERVER_ERROR", response.Message);
        }

        #endregion
    }
}
