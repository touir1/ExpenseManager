using Touir.ExpensesManager.Users.Controllers;
using Touir.ExpensesManager.Users.Controllers.Requests;
using Touir.ExpensesManager.Users.Controllers.Responses;
using Touir.ExpensesManager.Users.Services.Contracts;
using Touir.ExpensesManager.Users.Controllers.EO;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Touir.ExpensesManager.Users.Tests.Controllers
{
    public class AuthenticationControllerTests
    {
        #region RegisterAsync Tests

        [Fact]
        public async Task RegisterAsync_ReturnsUnauthorized_WhenRequestIsNull()
        {
            var controller = new AuthenticationController(Mock.Of<IAuthenticationService>(), Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var result = await controller.RegisterAsync(null!);
            
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task RegisterAsync_ReturnsUnauthorized_WhenFirstNameIsEmpty()
        {
            var controller = new AuthenticationController(Mock.Of<IAuthenticationService>(), Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var request = new RegisterRequest { FirstName = "", LastName = "Doe", Email = "john@doe.com", ApplicationCode = "APP1" };
            var result = await controller.RegisterAsync(request);
            
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task RegisterAsync_ReturnsUnauthorized_WhenLastNameIsEmpty()
        {
            var controller = new AuthenticationController(Mock.Of<IAuthenticationService>(), Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var request = new RegisterRequest { FirstName = "John", LastName = "", Email = "john@doe.com", ApplicationCode = "APP1" };
            var result = await controller.RegisterAsync(request);
            
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task RegisterAsync_ReturnsUnauthorized_WhenEmailIsEmpty()
        {
            var controller = new AuthenticationController(Mock.Of<IAuthenticationService>(), Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var request = new RegisterRequest { FirstName = "John", LastName = "Doe", Email = "", ApplicationCode = "APP1" };
            var result = await controller.RegisterAsync(request);
            
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task RegisterAsync_ReturnsOk_WhenValid()
        {
            var mockAuthService = new Mock<IAuthenticationService>();
            mockAuthService.Setup(s => s.RegisterNewUserAsync("John", "Doe", "john@doe.com", "APP1"))
                .ReturnsAsync(new List<string>());
            var controller = new AuthenticationController(mockAuthService.Object, Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var request = new RegisterRequest { FirstName = "John", LastName = "Doe", Email = "john@doe.com", ApplicationCode = "APP1" };
            var result = await controller.RegisterAsync(request);
            
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<RegisterResponse>(okResult.Value);
            
            Assert.False(response.HasError);
        }

        [Fact]
        public async Task RegisterAsync_ReturnsOkWithErrors_WhenServiceReturnsErrors()
        {
            var mockAuthService = new Mock<IAuthenticationService>();
            mockAuthService.Setup(s => s.RegisterNewUserAsync("John", "Doe", "john@doe.com", "APP1"))
                .ReturnsAsync(new List<string> { "email is already used" });
            var controller = new AuthenticationController(mockAuthService.Object, Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var request = new RegisterRequest { FirstName = "John", LastName = "Doe", Email = "john@doe.com", ApplicationCode = "APP1" };
            var result = await controller.RegisterAsync(request);
            
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<RegisterResponse>(okResult.Value);
            
            Assert.True(response.HasError);
            Assert.NotNull(response.Errors);
            Assert.Single(response.Errors);
        }

        [Fact]
        public async Task RegisterAsync_ReturnsBadRequest_WhenExceptionThrown()
        {
            var mockAuthService = new Mock<IAuthenticationService>();
            mockAuthService.Setup(s => s.RegisterNewUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Database error"));
            var controller = new AuthenticationController(mockAuthService.Object, Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var request = new RegisterRequest { FirstName = "John", LastName = "Doe", Email = "john@doe.com", ApplicationCode = "APP1" };
            var result = await controller.RegisterAsync(request);
            
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(badRequestResult.Value);
            Assert.Equal("SERVER_ERROR", response.Message);
        }

        #endregion

        #region LoginAsync Tests

        [Fact]
        public async Task LoginAsync_ReturnsUnauthorized_WhenRequestIsNull()
        {
            var controller = new AuthenticationController(Mock.Of<IAuthenticationService>(), Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var result = await controller.LoginAsync(null!);
            
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task LoginAsync_ReturnsUnauthorized_WhenApplicationCodeIsEmpty()
        {
            var controller = new AuthenticationController(Mock.Of<IAuthenticationService>(), Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var request = new LoginRequest { Email = "john@doe.com", Password = "password", ApplicationCode = "" };
            var result = await controller.LoginAsync(request);
            
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task LoginAsync_ReturnsUnauthorized_WhenEmailIsEmpty()
        {
            var controller = new AuthenticationController(Mock.Of<IAuthenticationService>(), Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var request = new LoginRequest { Email = "", Password = "password", ApplicationCode = "APP1" };
            var result = await controller.LoginAsync(request);
            
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task LoginAsync_ReturnsUnauthorized_WhenPasswordIsEmpty()
        {
            var controller = new AuthenticationController(Mock.Of<IAuthenticationService>(), Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var request = new LoginRequest { Email = "john@doe.com", Password = "", ApplicationCode = "APP1" };
            var result = await controller.LoginAsync(request);
            
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task LoginAsync_ReturnsUnauthorized_WhenUserNotFound()
        {
            var mockAuthService = new Mock<IAuthenticationService>();
            mockAuthService.Setup(s => s.AuthenticateAsync("john@doe.com", "password")).ReturnsAsync((UserEo?)null);
            var controller = new AuthenticationController(mockAuthService.Object, Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var request = new LoginRequest { Email = "john@doe.com", Password = "password", ApplicationCode = "APP1" };
            var result = await controller.LoginAsync(request);
            
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("INVALID_USERNAME_OR_PASSWORD", response.Message);
        }

        [Fact]
        public async Task LoginAsync_ReturnsUnauthorized_WhenNoRolesAssigned()
        {
            var user = new UserEo { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@doe.com" };
            var mockAuthService = new Mock<IAuthenticationService>();
            mockAuthService.Setup(s => s.AuthenticateAsync("john@doe.com", "password")).ReturnsAsync(user);
            var mockRoleService = new Mock<IRoleService>();
            mockRoleService.Setup(s => s.GetUserRolesByApplicationCodeAsync("APP1", user.Id.Value)).ReturnsAsync(new List<RoleEo>());
            var controller = new AuthenticationController(mockAuthService.Object, mockRoleService.Object, Mock.Of<IApplicationService>());
            var request = new LoginRequest { Email = "john@doe.com", Password = "password", ApplicationCode = "APP1" };
            var result = await controller.LoginAsync(request);
            
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("NO_ASSIGNED_ROLE", response.Message);
        }

        [Fact]
        public async Task LoginAsync_ReturnsOk_WhenValid()
        {
            var user = new UserEo { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@doe.com" };
            var mockAuthService = new Mock<IAuthenticationService>();
            mockAuthService.Setup(s => s.AuthenticateAsync("john@doe.com", "password")).ReturnsAsync(user);
            mockAuthService.Setup(s => s.GenerateJwtToken(user.Id.Value, user.Email)).Returns("token");
            var mockRoleService = new Mock<IRoleService>();
            mockRoleService.Setup(s => s.GetUserRolesByApplicationCodeAsync("APP1", user.Id.Value)).ReturnsAsync(new List<RoleEo> { new RoleEo { Code = "ADMIN", Name = "Admin" } });
            var controller = new AuthenticationController(mockAuthService.Object, mockRoleService.Object, Mock.Of<IApplicationService>());
            var request = new LoginRequest { Email = "john@doe.com", Password = "password", ApplicationCode = "APP1" };
            var result = await controller.LoginAsync(request);
            
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<LoginResponse>(okResult.Value);
            
            Assert.Equal("john@doe.com", response.User?.Email);
            Assert.Equal("token", response.Token);
        }

        [Fact]
        public async Task LoginAsync_ReturnsBadRequest_WhenExceptionThrown()
        {
            var mockAuthService = new Mock<IAuthenticationService>();
            mockAuthService.Setup(s => s.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Database error"));
            var controller = new AuthenticationController(mockAuthService.Object, Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var request = new LoginRequest { Email = "john@doe.com", Password = "password", ApplicationCode = "APP1" };
            var result = await controller.LoginAsync(request);
            
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(badRequestResult.Value);
            Assert.Equal("SERVER_ERROR", response.Message);
        }

        #endregion

        #region ValidateEmail Tests

        [Fact]
        public async Task ValidateEmail_ReturnsUnauthorized_WhenHashIsEmpty()
        {
            var controller = new AuthenticationController(Mock.Of<IAuthenticationService>(), Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var result = await controller.ValidateEmail("", "john@doe.com", "APP1");
            
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task ValidateEmail_ReturnsUnauthorized_WhenEmailIsEmpty()
        {
            var controller = new AuthenticationController(Mock.Of<IAuthenticationService>(), Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var result = await controller.ValidateEmail("hash", "", "APP1");
            
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task ValidateEmail_ReturnsUnauthorized_WhenAppCodeIsEmpty()
        {
            var controller = new AuthenticationController(Mock.Of<IAuthenticationService>(), Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var result = await controller.ValidateEmail("hash", "john@doe.com", "");
            
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task ValidateEmail_ReturnsUnauthorized_WhenApplicationNotFound()
        {
            var mockAppService = new Mock<IApplicationService>();
            mockAppService.Setup(s => s.GetApplicationByCodeAsync("APP1")).ReturnsAsync((ApplicationEo?)null);
            var controller = new AuthenticationController(Mock.Of<IAuthenticationService>(), Mock.Of<IRoleService>(), mockAppService.Object);
            var result = await controller.ValidateEmail("hash", "john@doe.com", "APP1");
            
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("EMAIL_VERIFICATION_FAILED", response.Message);
        }

        [Fact]
        public async Task ValidateEmail_ReturnsUnauthorized_WhenValidationFails()
        {
            var app = new ApplicationEo { Id = 1, Code = "APP1", Name = "App1", ResetPasswordUrlPath = "http://reset" };
            var mockAppService = new Mock<IApplicationService>();
            mockAppService.Setup(s => s.GetApplicationByCodeAsync("APP1")).ReturnsAsync(app);
            var mockAuthService = new Mock<IAuthenticationService>();
            mockAuthService.Setup(s => s.ValidateEmailAsync("hash", "john@doe.com")).ReturnsAsync(false);
            var controller = new AuthenticationController(mockAuthService.Object, Mock.Of<IRoleService>(), mockAppService.Object);
            var result = await controller.ValidateEmail("hash", "john@doe.com", "APP1");
            
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("EMAIL_VERIFICATION_FAILED", response.Message);
        }

        [Fact]
        public async Task ValidateEmail_ReturnsRedirect_WhenValid()
        {
            var app = new ApplicationEo { Id = 1, Code = "APP1", Name = "App1", ResetPasswordUrlPath = "http://reset" };
            var mockAppService = new Mock<IApplicationService>();
            mockAppService.Setup(s => s.GetApplicationByCodeAsync("APP1")).ReturnsAsync(app);
            var mockAuthService = new Mock<IAuthenticationService>();
            mockAuthService.Setup(s => s.ValidateEmailAsync("hash", "john@doe.com")).ReturnsAsync(true);
            var controller = new AuthenticationController(mockAuthService.Object, Mock.Of<IRoleService>(), mockAppService.Object);
            
            var result = await controller.ValidateEmail("hash", "john@doe.com", "APP1");
            var redirectResult = Assert.IsType<RedirectResult>(result);
            
            Assert.StartsWith("http://reset", redirectResult.Url);
        }

        [Fact]
        public async Task ValidateEmail_ReturnsBadRequest_WhenExceptionThrown()
        {
            var mockAppService = new Mock<IApplicationService>();
            mockAppService.Setup(s => s.GetApplicationByCodeAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Database error"));
            var controller = new AuthenticationController(Mock.Of<IAuthenticationService>(), Mock.Of<IRoleService>(), mockAppService.Object);
            var result = await controller.ValidateEmail("hash", "john@doe.com", "APP1");
            
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(badRequestResult.Value);
            Assert.Equal("SERVER_ERROR", response.Message);
        }

        #endregion

        #region ChangePassword Tests

        [Fact]
        public async Task ChangePassword_ReturnsUnauthorized_WhenRequestIsNull()
        {
            var controller = new AuthenticationController(Mock.Of<IAuthenticationService>(), Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var result = await controller.ChangePassword(null!);
            
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task ChangePassword_ReturnsUnauthorized_WhenEmailIsEmpty()
        {
            var controller = new AuthenticationController(Mock.Of<IAuthenticationService>(), Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var request = new ChangePasswordRequest { Email = "", OldPassword = "old", NewPassword = "new", ConfirmPassword = "new" };
            var result = await controller.ChangePassword(request);
            
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task ChangePassword_ReturnsUnauthorized_WhenOldPasswordIsEmpty()
        {
            var controller = new AuthenticationController(Mock.Of<IAuthenticationService>(), Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var request = new ChangePasswordRequest { Email = "john@doe.com", OldPassword = "", NewPassword = "new", ConfirmPassword = "new" };
            var result = await controller.ChangePassword(request);
            
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task ChangePassword_ReturnsUnauthorized_WhenNewPasswordIsEmpty()
        {
            var controller = new AuthenticationController(Mock.Of<IAuthenticationService>(), Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var request = new ChangePasswordRequest { Email = "john@doe.com", OldPassword = "old", NewPassword = "", ConfirmPassword = "new" };
            var result = await controller.ChangePassword(request);
            
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task ChangePassword_ReturnsUnauthorized_WhenConfirmPasswordIsEmpty()
        {
            var controller = new AuthenticationController(Mock.Of<IAuthenticationService>(), Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var request = new ChangePasswordRequest { Email = "john@doe.com", OldPassword = "old", NewPassword = "new", ConfirmPassword = "" };
            var result = await controller.ChangePassword(request);
            
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task ChangePassword_ReturnsUnauthorized_WhenPasswordsDoNotMatch()
        {
            var controller = new AuthenticationController(Mock.Of<IAuthenticationService>(), Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var request = new ChangePasswordRequest { Email = "john@doe.com", OldPassword = "old", NewPassword = "new", ConfirmPassword = "different" };
            var result = await controller.ChangePassword(request);
            
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("NOT_MATCHING_CONFIRM_PASSWORD", response.Message);
        }

        [Fact]
        public async Task ChangePassword_ReturnsUnauthorized_WhenServiceReturnsFalse()
        {
            var mockAuthService = new Mock<IAuthenticationService>();
            mockAuthService.Setup(s => s.ChangePasswordAsync("john@doe.com", "old", "new")).ReturnsAsync(false);
            var controller = new AuthenticationController(mockAuthService.Object, Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var request = new ChangePasswordRequest { Email = "john@doe.com", OldPassword = "old", NewPassword = "new", ConfirmPassword = "new" };
            var result = await controller.ChangePassword(request);
            
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("SET_NEW_PASSWORD_FAILED", response.Message);
        }

        [Fact]
        public async Task ChangePassword_ReturnsOk_WhenValid()
        {
            var mockAuthService = new Mock<IAuthenticationService>();
            mockAuthService.Setup(s => s.ChangePasswordAsync("john@doe.com", "old", "new")).ReturnsAsync(true);
            var controller = new AuthenticationController(mockAuthService.Object, Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var request = new ChangePasswordRequest { Email = "john@doe.com", OldPassword = "old", NewPassword = "new", ConfirmPassword = "new" };
            
            var result = await controller.ChangePassword(request);
            
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task ChangePassword_ReturnsBadRequest_WhenExceptionThrown()
        {
            var mockAuthService = new Mock<IAuthenticationService>();
            mockAuthService.Setup(s => s.ChangePasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Database error"));
            var controller = new AuthenticationController(mockAuthService.Object, Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
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
            var controller = new AuthenticationController(Mock.Of<IAuthenticationService>(), Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var result = await controller.RequestPasswordReset(null!);
            
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task RequestPasswordReset_ReturnsUnauthorized_WhenEmailIsEmpty()
        {
            var controller = new AuthenticationController(Mock.Of<IAuthenticationService>(), Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var request = new RequestPasswordResetRequest { Email = "" };
            var result = await controller.RequestPasswordReset(request);
            
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task RequestPasswordReset_ReturnsUnauthorized_WhenServiceReturnsFalse()
        {
            var mockAuthService = new Mock<IAuthenticationService>();
            mockAuthService.Setup(s => s.RequestPasswordResetAsync("john@doe.com")).ReturnsAsync(false);
            var controller = new AuthenticationController(mockAuthService.Object, Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var request = new RequestPasswordResetRequest { Email = "john@doe.com" };
            var result = await controller.RequestPasswordReset(request);
            
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("REQUEST_PASSWORD_RESET_FAILED", response.Message);
        }

        [Fact]
        public async Task RequestPasswordReset_ReturnsOk_WhenValid()
        {
            var mockAuthService = new Mock<IAuthenticationService>();
            mockAuthService.Setup(s => s.RequestPasswordResetAsync("john@doe.com")).ReturnsAsync(true);
            var controller = new AuthenticationController(mockAuthService.Object, Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var request = new RequestPasswordResetRequest { Email = "john@doe.com" };
            
            var result = await controller.RequestPasswordReset(request);
            
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task RequestPasswordReset_ReturnsBadRequest_WhenExceptionThrown()
        {
            var mockAuthService = new Mock<IAuthenticationService>();
            mockAuthService.Setup(s => s.RequestPasswordResetAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Database error"));
            var controller = new AuthenticationController(mockAuthService.Object, Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var request = new RequestPasswordResetRequest { Email = "john@doe.com" };
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
            var controller = new AuthenticationController(Mock.Of<IAuthenticationService>(), Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var result = await controller.ChangePasswordReset(null!);
            
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task ChangePasswordReset_ReturnsUnauthorized_WhenEmailIsEmpty()
        {
            var controller = new AuthenticationController(Mock.Of<IAuthenticationService>(), Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var request = new ChangePasswordResetRequest { Email = "", VerificationHash = "hash", NewPassword = "new", ConfirmPassword = "new" };
            var result = await controller.ChangePasswordReset(request);
            
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task ChangePasswordReset_ReturnsUnauthorized_WhenVerificationHashIsEmpty()
        {
            var controller = new AuthenticationController(Mock.Of<IAuthenticationService>(), Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var request = new ChangePasswordResetRequest { Email = "john@doe.com", VerificationHash = "", NewPassword = "new", ConfirmPassword = "new" };
            var result = await controller.ChangePasswordReset(request);
            
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task ChangePasswordReset_ReturnsUnauthorized_WhenNewPasswordIsEmpty()
        {
            var controller = new AuthenticationController(Mock.Of<IAuthenticationService>(), Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var request = new ChangePasswordResetRequest { Email = "john@doe.com", VerificationHash = "hash", NewPassword = "", ConfirmPassword = "new" };
            var result = await controller.ChangePasswordReset(request);
            
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task ChangePasswordReset_ReturnsUnauthorized_WhenConfirmPasswordIsEmpty()
        {
            var controller = new AuthenticationController(Mock.Of<IAuthenticationService>(), Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var request = new ChangePasswordResetRequest { Email = "john@doe.com", VerificationHash = "hash", NewPassword = "new", ConfirmPassword = "" };
            var result = await controller.ChangePasswordReset(request);
            
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task ChangePasswordReset_ReturnsUnauthorized_WhenPasswordsDoNotMatch()
        {
            var controller = new AuthenticationController(Mock.Of<IAuthenticationService>(), Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var request = new ChangePasswordResetRequest { Email = "john@doe.com", VerificationHash = "hash", NewPassword = "new", ConfirmPassword = "different" };
            var result = await controller.ChangePasswordReset(request);
            
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("NOT_MATCHING_CONFIRM_PASSWORD", response.Message);
        }

        [Fact]
        public async Task ChangePasswordReset_ReturnsUnauthorized_WhenServiceReturnsFalse()
        {
            var mockAuthService = new Mock<IAuthenticationService>();
            mockAuthService.Setup(s => s.ResetPasswordAsync("john@doe.com", "hash", "new")).ReturnsAsync(false);
            var controller = new AuthenticationController(mockAuthService.Object, Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var request = new ChangePasswordResetRequest { Email = "john@doe.com", VerificationHash = "hash", NewPassword = "new", ConfirmPassword = "new" };
            var result = await controller.ChangePasswordReset(request);
            
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("RESET_PASSWORD_FAILED", response.Message);
        }

        [Fact]
        public async Task ChangePasswordReset_ReturnsOk_WhenValid()
        {
            var mockAuthService = new Mock<IAuthenticationService>();
            mockAuthService.Setup(s => s.ResetPasswordAsync("john@doe.com", "hash", "new")).ReturnsAsync(true);
            var controller = new AuthenticationController(mockAuthService.Object, Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var request = new ChangePasswordResetRequest { Email = "john@doe.com", VerificationHash = "hash", NewPassword = "new", ConfirmPassword = "new" };
            
            var result = await controller.ChangePasswordReset(request);
            
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task ChangePasswordReset_ReturnsBadRequest_WhenExceptionThrown()
        {
            var mockAuthService = new Mock<IAuthenticationService>();
            mockAuthService.Setup(s => s.ResetPasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Database error"));
            var controller = new AuthenticationController(mockAuthService.Object, Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var request = new ChangePasswordResetRequest { Email = "john@doe.com", VerificationHash = "hash", NewPassword = "new", ConfirmPassword = "new" };
            var result = await controller.ChangePasswordReset(request);
            
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(badRequestResult.Value);
            Assert.Equal("SERVER_ERROR", response.Message);
        }

        #endregion
    }
}
