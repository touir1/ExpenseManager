using Touir.ExpensesManager.Users.Controllers;
using Touir.ExpensesManager.Users.Controllers.Requests;
using Touir.ExpensesManager.Users.Controllers.Responses;
using Touir.ExpensesManager.Users.Services.Contracts;
using Touir.ExpensesManager.Users.Repositories.Contracts;
using Touir.ExpensesManager.Users.Controllers.EO;
using Touir.ExpensesManager.Users.Infrastructure.Options;
using Touir.ExpensesManager.Users.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;

namespace Touir.ExpensesManager.Users.Tests.Controllers
{
    public class AuthenticationControllerTests
    {
        private static AuthenticationController CreateController(
            IAuthenticationService? authService = null,
            IRefreshTokenService? refreshTokenService = null,
            IUserRepository? userRepository = null,
            IRoleService? roleService = null,
            IApplicationService? appService = null,
            JwtAuthOptions? options = null)
        {
            return new AuthenticationController(
                authService ?? Mock.Of<IAuthenticationService>(),
                refreshTokenService ?? Mock.Of<IRefreshTokenService>(),
                userRepository ?? Mock.Of<IUserRepository>(),
                roleService ?? Mock.Of<IRoleService>(),
                appService ?? Mock.Of<IApplicationService>(),
                Options.Create(options ?? new JwtAuthOptions()));
        }

        #region RegisterAsync Tests

        [Fact]
        public async Task RegisterAsync_ReturnsUnauthorized_WhenRequestIsNull()
        {
            var controller = CreateController();
            var result = await controller.RegisterAsync(null!);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task RegisterAsync_ReturnsUnauthorized_WhenFirstNameIsEmpty()
        {
            var controller = CreateController();
            var request = new RegisterRequest { FirstName = "", LastName = "Doe", Email = "john@doe.com", ApplicationCode = "APP1" };
            var result = await controller.RegisterAsync(request);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task RegisterAsync_ReturnsUnauthorized_WhenLastNameIsEmpty()
        {
            var controller = CreateController();
            var request = new RegisterRequest { FirstName = "John", LastName = "", Email = "john@doe.com", ApplicationCode = "APP1" };
            var result = await controller.RegisterAsync(request);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task RegisterAsync_ReturnsUnauthorized_WhenEmailIsEmpty()
        {
            var controller = CreateController();
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
            var controller = CreateController(authService: mockAuthService.Object);
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
            var controller = CreateController(authService: mockAuthService.Object);
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
            var controller = CreateController(authService: mockAuthService.Object);
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
            var controller = CreateController();
            var result = await controller.LoginAsync(null!);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task LoginAsync_ReturnsUnauthorized_WhenApplicationCodeIsEmpty()
        {
            var controller = CreateController();
            var request = new LoginRequest { Email = "john@doe.com", Password = "password", ApplicationCode = "" };
            var result = await controller.LoginAsync(request);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task LoginAsync_ReturnsUnauthorized_WhenEmailIsEmpty()
        {
            var controller = CreateController();
            var request = new LoginRequest { Email = "", Password = "password", ApplicationCode = "APP1" };
            var result = await controller.LoginAsync(request);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task LoginAsync_ReturnsUnauthorized_WhenPasswordIsEmpty()
        {
            var controller = CreateController();
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
            var controller = CreateController(authService: mockAuthService.Object);
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
            mockRoleService.Setup(s => s.GetUserRolesByApplicationCodeAsync("APP1", user.Id!.Value)).ReturnsAsync(new List<RoleEo>());
            var controller = CreateController(authService: mockAuthService.Object, roleService: mockRoleService.Object);
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
            mockAuthService.Setup(s => s.GenerateJwtToken(user.Id!.Value, user.Email, user.FirstName, user.LastName)).Returns("access_token");
            var mockRoleService = new Mock<IRoleService>();
            mockRoleService.Setup(s => s.GetUserRolesByApplicationCodeAsync("APP1", user.Id!.Value))
                .ReturnsAsync(new List<RoleEo> { new RoleEo { Code = "ADMIN", Name = "Admin" } });
            var mockRefreshService = new Mock<IRefreshTokenService>();
            mockRefreshService.Setup(s => s.GenerateAsync(user.Id!.Value, It.IsAny<bool>())).ReturnsAsync("refresh_token_value");
            var controller = CreateController(
                authService: mockAuthService.Object,
                refreshTokenService: mockRefreshService.Object,
                roleService: mockRoleService.Object,
                options: new JwtAuthOptions { ExpiryInMinutes = 60 });
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
            var request = new LoginRequest { Email = "john@doe.com", Password = "password", ApplicationCode = "APP1" };
            var result = await controller.LoginAsync(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<LoginResponse>(okResult.Value);
            Assert.Equal("john@doe.com", response.User?.Email);
        }

        [Fact]
        public async Task LoginAsync_SetsHttpOnlyCookies_WhenValid()
        {
            var user = new UserEo { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@doe.com" };
            var mockAuthService = new Mock<IAuthenticationService>();
            mockAuthService.Setup(s => s.AuthenticateAsync("john@doe.com", "password")).ReturnsAsync(user);
            mockAuthService.Setup(s => s.GenerateJwtToken(user.Id!.Value, user.Email, user.FirstName, user.LastName)).Returns("access_token");
            var mockRoleService = new Mock<IRoleService>();
            mockRoleService.Setup(s => s.GetUserRolesByApplicationCodeAsync("APP1", user.Id!.Value))
                .ReturnsAsync(new List<RoleEo> { new RoleEo { Code = "ADMIN", Name = "Admin" } });
            var mockRefreshService = new Mock<IRefreshTokenService>();
            mockRefreshService.Setup(s => s.GenerateAsync(user.Id!.Value, It.IsAny<bool>())).ReturnsAsync("refresh_token_value");
            var httpContext = new DefaultHttpContext();
            var controller = CreateController(
                authService: mockAuthService.Object,
                refreshTokenService: mockRefreshService.Object,
                roleService: mockRoleService.Object,
                options: new JwtAuthOptions { ExpiryInMinutes = 60 });
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var request = new LoginRequest { Email = "john@doe.com", Password = "password", ApplicationCode = "APP1" };

            await controller.LoginAsync(request);

            var cookieHeader = httpContext.Response.Headers.SetCookie.ToString();
            Assert.Contains("auth_token", cookieHeader);
            Assert.Contains("refresh_token", cookieHeader);
            Assert.Contains("httponly", cookieHeader, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("secure", cookieHeader, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task LoginAsync_SetsSessionCookies_WhenRememberMeFalse()
        {
            var user = new UserEo { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@doe.com" };
            var mockAuthService = new Mock<IAuthenticationService>();
            mockAuthService.Setup(s => s.AuthenticateAsync("john@doe.com", "password")).ReturnsAsync(user);
            mockAuthService.Setup(s => s.GenerateJwtToken(user.Id!.Value, user.Email, user.FirstName, user.LastName)).Returns("access_token");
            var mockRoleService = new Mock<IRoleService>();
            mockRoleService.Setup(s => s.GetUserRolesByApplicationCodeAsync("APP1", user.Id!.Value))
                .ReturnsAsync(new List<RoleEo> { new RoleEo { Code = "ADMIN", Name = "Admin" } });
            var mockRefreshService = new Mock<IRefreshTokenService>();
            mockRefreshService.Setup(s => s.GenerateAsync(user.Id!.Value, false)).ReturnsAsync("refresh_token_value");
            var httpContext = new DefaultHttpContext();
            var controller = CreateController(
                authService: mockAuthService.Object,
                refreshTokenService: mockRefreshService.Object,
                roleService: mockRoleService.Object,
                options: new JwtAuthOptions { ExpiryInMinutes = 60 });
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var request = new LoginRequest { Email = "john@doe.com", Password = "password", ApplicationCode = "APP1", RememberMe = false };

            await controller.LoginAsync(request);

            // Session cookies have no expires attribute
            var cookieHeader = httpContext.Response.Headers.SetCookie.ToString();
            Assert.DoesNotContain("expires=", cookieHeader, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task LoginAsync_SetsPersistentCookies_WhenRememberMeTrue()
        {
            var user = new UserEo { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@doe.com" };
            var mockAuthService = new Mock<IAuthenticationService>();
            mockAuthService.Setup(s => s.AuthenticateAsync("john@doe.com", "password")).ReturnsAsync(user);
            mockAuthService.Setup(s => s.GenerateJwtToken(user.Id!.Value, user.Email, user.FirstName, user.LastName)).Returns("access_token");
            var mockRoleService = new Mock<IRoleService>();
            mockRoleService.Setup(s => s.GetUserRolesByApplicationCodeAsync("APP1", user.Id!.Value))
                .ReturnsAsync(new List<RoleEo> { new RoleEo { Code = "ADMIN", Name = "Admin" } });
            var mockRefreshService = new Mock<IRefreshTokenService>();
            mockRefreshService.Setup(s => s.GenerateAsync(user.Id!.Value, true)).ReturnsAsync("refresh_token_value");
            var httpContext = new DefaultHttpContext();
            var controller = CreateController(
                authService: mockAuthService.Object,
                refreshTokenService: mockRefreshService.Object,
                roleService: mockRoleService.Object,
                options: new JwtAuthOptions { ExpiryInMinutes = 60 });
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var request = new LoginRequest { Email = "john@doe.com", Password = "password", ApplicationCode = "APP1", RememberMe = true };

            await controller.LoginAsync(request);

            // Persistent cookies have expires attribute
            var cookieHeader = httpContext.Response.Headers.SetCookie.ToString();
            Assert.Contains("expires=", cookieHeader, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task LoginAsync_ReturnsBadRequest_WhenExceptionThrown()
        {
            var mockAuthService = new Mock<IAuthenticationService>();
            mockAuthService.Setup(s => s.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Database error"));
            var controller = CreateController(authService: mockAuthService.Object);
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
            var controller = CreateController();
            var result = await controller.ValidateEmail("", "john@doe.com", "APP1");

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task ValidateEmail_ReturnsUnauthorized_WhenEmailIsEmpty()
        {
            var controller = CreateController();
            var result = await controller.ValidateEmail("hash", "", "APP1");

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_PARAMETERS", response.Message);
        }

        [Fact]
        public async Task ValidateEmail_ReturnsUnauthorized_WhenAppCodeIsEmpty()
        {
            var controller = CreateController();
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
            var controller = CreateController(appService: mockAppService.Object);
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
            var controller = CreateController(authService: mockAuthService.Object, appService: mockAppService.Object);
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
            var controller = CreateController(authService: mockAuthService.Object, appService: mockAppService.Object);

            var result = await controller.ValidateEmail("hash", "john@doe.com", "APP1");
            var redirectResult = Assert.IsType<RedirectResult>(result);

            Assert.StartsWith("http://reset", redirectResult.Url);
            Assert.Contains("mode=create", redirectResult.Url);
        }

        [Fact]
        public async Task ValidateEmail_ReturnsBadRequest_WhenExceptionThrown()
        {
            var mockAppService = new Mock<IApplicationService>();
            mockAppService.Setup(s => s.GetApplicationByCodeAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Database error"));
            var controller = CreateController(appService: mockAppService.Object);
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
            var mockAuthService = new Mock<IAuthenticationService>();
            mockAuthService.Setup(s => s.ChangePasswordAsync("john@doe.com", "old", "new")).ReturnsAsync(false);
            var controller = CreateController(authService: mockAuthService.Object);
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
            var controller = CreateController(authService: mockAuthService.Object);
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
            var controller = CreateController(authService: mockAuthService.Object);
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
            var controller = CreateController(authService: mockAuthService.Object);
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
            var controller = CreateController(authService: mockAuthService.Object);
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
            var controller = CreateController(authService: mockAuthService.Object);
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
            var mockAuthService = new Mock<IAuthenticationService>();
            mockAuthService.Setup(s => s.ResetPasswordAsync("john@doe.com", "hash", "new")).ReturnsAsync(false);
            var controller = CreateController(authService: mockAuthService.Object);
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
            var controller = CreateController(authService: mockAuthService.Object);
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
            var controller = CreateController(authService: mockAuthService.Object);
            var request = new ChangePasswordResetRequest { Email = "john@doe.com", VerificationHash = "hash", NewPassword = "new", ConfirmPassword = "new" };
            var result = await controller.ChangePasswordReset(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(badRequestResult.Value);
            Assert.Equal("SERVER_ERROR", response.Message);
        }

        #endregion

        #region Check Tests

        [Fact]
        public void Check_ReturnsUnauthorized_WhenAuthorizationHeaderIsMissing()
        {
            var controller = CreateController();
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            var result = controller.Check();

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_TOKEN", response.Message);
        }

        [Fact]
        public void Check_ReturnsUnauthorized_WhenAuthorizationHeaderIsInvalidFormat()
        {
            var controller = CreateController();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Authorization = "Basic token";
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = controller.Check();

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_TOKEN", response.Message);
        }

        [Fact]
        public void Check_ReturnsUnauthorized_WhenTokenIsInvalid()
        {
            var mockAuthService = new Mock<IAuthenticationService>();
            mockAuthService.Setup(s => s.ValidateToken("invalid_token")).Returns(new Microsoft.IdentityModel.Tokens.TokenValidationResult { IsValid = false });
            var controller = CreateController(authService: mockAuthService.Object);
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Authorization = "Bearer invalid_token";
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = controller.Check();

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("INVALID_TOKEN", response.Message);
        }

        [Fact]
        public void Check_ReturnsOk_WhenTokenIsValid()
        {
            var mockAuthService = new Mock<IAuthenticationService>();
            mockAuthService.Setup(s => s.ValidateToken("valid_token")).Returns(new Microsoft.IdentityModel.Tokens.TokenValidationResult { IsValid = true });
            var controller = CreateController(authService: mockAuthService.Object);
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Authorization = "Bearer valid_token";
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = controller.Check();

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public void Check_ReturnsBadRequest_WhenExceptionThrown()
        {
            var mockAuthService = new Mock<IAuthenticationService>();
            mockAuthService.Setup(s => s.ValidateToken(It.IsAny<string>())).Throws(new Exception("Database error"));
            var controller = CreateController(authService: mockAuthService.Object);
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Authorization = "Bearer some_token";
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = controller.Check();

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(badRequestResult.Value);
            Assert.Equal("SERVER_ERROR", response.Message);
        }

        [Fact]
        public void Check_ReturnsUnauthorized_WhenCookieTokenIsInvalid()
        {
            var mockAuthService = new Mock<IAuthenticationService>();
            mockAuthService.Setup(s => s.ValidateToken("invalid_cookie_token")).Returns(new Microsoft.IdentityModel.Tokens.TokenValidationResult { IsValid = false });
            var controller = CreateController(authService: mockAuthService.Object);
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Cookie = "auth_token=invalid_cookie_token";
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = controller.Check();

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("INVALID_TOKEN", response.Message);
        }

        [Fact]
        public void Check_ReturnsOk_WhenCookieTokenIsValid()
        {
            var mockAuthService = new Mock<IAuthenticationService>();
            mockAuthService.Setup(s => s.ValidateToken("valid_cookie_token")).Returns(new Microsoft.IdentityModel.Tokens.TokenValidationResult { IsValid = true });
            var controller = CreateController(authService: mockAuthService.Object);
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Cookie = "auth_token=valid_cookie_token";
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = controller.Check();

            Assert.IsType<OkResult>(result);
        }

        #endregion

        #region Session Tests

        [Fact]
        public void Session_ReturnsUnauthorized_WhenCookieIsMissing()
        {
            var controller = CreateController();
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            var result = controller.Session();

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_TOKEN", response.Message);
        }

        [Fact]
        public void Session_ReturnsUnauthorized_WhenTokenIsInvalid()
        {
            var mockAuthService = new Mock<IAuthenticationService>();
            mockAuthService.Setup(s => s.ValidateToken("invalid_token")).Returns(new Microsoft.IdentityModel.Tokens.TokenValidationResult { IsValid = false });
            var controller = CreateController(authService: mockAuthService.Object);
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Cookie = "auth_token=invalid_token";
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = controller.Session();

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("INVALID_TOKEN", response.Message);
        }

        [Fact]
        public void Session_ReturnsOkWithUserData_WhenTokenIsValid()
        {
            var mockAuthService = new Mock<IAuthenticationService>();
            mockAuthService.Setup(s => s.ValidateToken("valid_token")).Returns(new Microsoft.IdentityModel.Tokens.TokenValidationResult { IsValid = true, SecurityToken = null! });
            var controller = CreateController(authService: mockAuthService.Object);
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Cookie = "auth_token=valid_token";
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = controller.Session();

            // Returns OkObjectResult with SessionResponse (email may be empty when SecurityToken is null)
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<SessionResponse>(okResult.Value);
        }

        [Fact]
        public void Session_ReturnsBadRequest_WhenExceptionThrown()
        {
            var mockAuthService = new Mock<IAuthenticationService>();
            mockAuthService.Setup(s => s.ValidateToken(It.IsAny<string>())).Throws(new Exception("Unexpected error"));
            var controller = CreateController(authService: mockAuthService.Object);
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Cookie = "auth_token=some_token";
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = controller.Session();

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(badRequestResult.Value);
            Assert.Equal("SERVER_ERROR", response.Message);
        }

        #endregion

        #region Refresh Tests

        [Fact]
        public async Task RefreshAsync_ReturnsUnauthorized_WhenRefreshCookieMissing()
        {
            var controller = CreateController();
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            var result = await controller.RefreshAsync();

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("MISSING_TOKEN", response.Message);
        }

        [Fact]
        public async Task RefreshAsync_ReturnsUnauthorized_WhenRefreshTokenInvalid()
        {
            var mockRefreshService = new Mock<IRefreshTokenService>();
            mockRefreshService.Setup(s => s.ValidateAsync("invalid_refresh")).ReturnsAsync((false, 0));
            var controller = CreateController(refreshTokenService: mockRefreshService.Object);
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Cookie = "refresh_token=invalid_refresh";
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = await controller.RefreshAsync();

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("INVALID_TOKEN", response.Message);
        }

        [Fact]
        public async Task RefreshAsync_ReturnsOkAndSetsCookies_WhenRefreshTokenValid()
        {
            var user = new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@doe.com" };
            var mockRefreshService = new Mock<IRefreshTokenService>();
            mockRefreshService.Setup(s => s.ValidateAsync("valid_refresh")).ReturnsAsync((true, 1));
            mockRefreshService.Setup(s => s.GenerateAsync(1, true)).ReturnsAsync("new_refresh_token");
            var mockUserRepo = new Mock<IUserRepository>();
            mockUserRepo.Setup(r => r.GetUserByIdAsync(1)).ReturnsAsync(user);
            var mockAuthService = new Mock<IAuthenticationService>();
            mockAuthService.Setup(s => s.GenerateJwtToken(1, user.Email, user.FirstName, user.LastName)).Returns("new_access_token");
            var httpContext = new DefaultHttpContext();
            var controller = CreateController(
                authService: mockAuthService.Object,
                refreshTokenService: mockRefreshService.Object,
                userRepository: mockUserRepo.Object,
                options: new JwtAuthOptions { ExpiryInMinutes = 60 });
            httpContext.Request.Headers.Cookie = "refresh_token=valid_refresh";
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = await controller.RefreshAsync();

            Assert.IsType<OkResult>(result);
            var cookieHeader = httpContext.Response.Headers.SetCookie.ToString();
            Assert.Contains("auth_token", cookieHeader);
            Assert.Contains("refresh_token", cookieHeader);
        }

        [Fact]
        public async Task RefreshAsync_ReturnsBadRequest_WhenExceptionThrown()
        {
            var mockRefreshService = new Mock<IRefreshTokenService>();
            mockRefreshService.Setup(s => s.ValidateAsync(It.IsAny<string>())).ThrowsAsync(new Exception("DB error"));
            var controller = CreateController(refreshTokenService: mockRefreshService.Object);
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Cookie = "refresh_token=some_token";
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = await controller.RefreshAsync();

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(badRequestResult.Value);
            Assert.Equal("SERVER_ERROR", response.Message);
        }

        #endregion

        #region Logout Tests

        [Fact]
        public async Task Logout_ReturnsOk_AndDeletesCookies()
        {
            var controller = CreateController();
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = await controller.Logout();

            Assert.IsType<OkResult>(result);
            var cookieHeader = httpContext.Response.Headers.SetCookie.ToString();
            Assert.Contains("auth_token", cookieHeader);
            Assert.Contains("refresh_token", cookieHeader);
        }

        #endregion
    }
}
