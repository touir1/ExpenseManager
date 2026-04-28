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
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Touir.ExpensesManager.Users.Tests.Controllers
{
    public class AuthenticationControllerTests
    {
        private static AuthenticationController CreateController(
            IAuthenticationService? authService = null,
            IJwtTokenService? jwtTokenService = null,
            IRefreshTokenService? refreshTokenService = null,
            IUserRepository? userRepository = null,
            IRoleService? roleService = null,
            JwtAuthOptions? options = null)
        {
            return new AuthenticationController(
                authService ?? Mock.Of<IAuthenticationService>(),
                jwtTokenService ?? Mock.Of<IJwtTokenService>(),
                refreshTokenService ?? Mock.Of<IRefreshTokenService>(),
                userRepository ?? Mock.Of<IUserRepository>(),
                roleService ?? Mock.Of<IRoleService>(),
                Options.Create(options ?? new JwtAuthOptions()));
        }

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
        public async Task LoginAsync_ReturnsUnauthorized_WhenRolesIsNull()
        {
            var user = new UserEo { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@doe.com" };
            var mockAuthService = new Mock<IAuthenticationService>();
            mockAuthService.Setup(s => s.AuthenticateAsync("john@doe.com", "password")).ReturnsAsync(user);
            var mockRoleService = new Mock<IRoleService>();
            mockRoleService.Setup(s => s.GetUserRolesByApplicationCodeAsync("APP1", user.Id!.Value)).Returns(Task.FromResult<IEnumerable<RoleEo>>(null!));
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
            var mockJwtService = new Mock<IJwtTokenService>();
            mockJwtService.Setup(s => s.GenerateJwtToken(user.Id!.Value, user.Email, user.FirstName, user.LastName)).Returns("access_token");
            var mockRoleService = new Mock<IRoleService>();
            mockRoleService.Setup(s => s.GetUserRolesByApplicationCodeAsync("APP1", user.Id!.Value))
                .ReturnsAsync(new List<RoleEo> { new RoleEo { Code = "ADMIN", Name = "Admin" } });
            var mockRefreshService = new Mock<IRefreshTokenService>();
            mockRefreshService.Setup(s => s.GenerateAsync(user.Id!.Value, It.IsAny<bool>())).ReturnsAsync("refresh_token_value");
            var controller = CreateController(
                authService: mockAuthService.Object,
                jwtTokenService: mockJwtService.Object,
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
            var mockJwtService = new Mock<IJwtTokenService>();
            mockJwtService.Setup(s => s.GenerateJwtToken(user.Id!.Value, user.Email, user.FirstName, user.LastName)).Returns("access_token");
            var mockRoleService = new Mock<IRoleService>();
            mockRoleService.Setup(s => s.GetUserRolesByApplicationCodeAsync("APP1", user.Id!.Value))
                .ReturnsAsync(new List<RoleEo> { new RoleEo { Code = "ADMIN", Name = "Admin" } });
            var mockRefreshService = new Mock<IRefreshTokenService>();
            mockRefreshService.Setup(s => s.GenerateAsync(user.Id!.Value, It.IsAny<bool>())).ReturnsAsync("refresh_token_value");
            var httpContext = new DefaultHttpContext();
            var controller = CreateController(
                authService: mockAuthService.Object,
                jwtTokenService: mockJwtService.Object,
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
            var mockJwtService = new Mock<IJwtTokenService>();
            mockJwtService.Setup(s => s.GenerateJwtToken(user.Id!.Value, user.Email, user.FirstName, user.LastName)).Returns("access_token");
            var mockRoleService = new Mock<IRoleService>();
            mockRoleService.Setup(s => s.GetUserRolesByApplicationCodeAsync("APP1", user.Id!.Value))
                .ReturnsAsync(new List<RoleEo> { new RoleEo { Code = "ADMIN", Name = "Admin" } });
            var mockRefreshService = new Mock<IRefreshTokenService>();
            mockRefreshService.Setup(s => s.GenerateAsync(user.Id!.Value, false)).ReturnsAsync("refresh_token_value");
            var httpContext = new DefaultHttpContext();
            var controller = CreateController(
                authService: mockAuthService.Object,
                jwtTokenService: mockJwtService.Object,
                refreshTokenService: mockRefreshService.Object,
                roleService: mockRoleService.Object,
                options: new JwtAuthOptions { ExpiryInMinutes = 60 });
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var request = new LoginRequest { Email = "john@doe.com", Password = "password", ApplicationCode = "APP1", RememberMe = false };

            await controller.LoginAsync(request);

            var cookieHeader = httpContext.Response.Headers.SetCookie.ToString();
            Assert.DoesNotContain("expires=", cookieHeader, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task LoginAsync_SetsPersistentCookies_WhenRememberMeTrue()
        {
            var user = new UserEo { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@doe.com" };
            var mockAuthService = new Mock<IAuthenticationService>();
            mockAuthService.Setup(s => s.AuthenticateAsync("john@doe.com", "password")).ReturnsAsync(user);
            var mockJwtService = new Mock<IJwtTokenService>();
            mockJwtService.Setup(s => s.GenerateJwtToken(user.Id!.Value, user.Email, user.FirstName, user.LastName)).Returns("access_token");
            var mockRoleService = new Mock<IRoleService>();
            mockRoleService.Setup(s => s.GetUserRolesByApplicationCodeAsync("APP1", user.Id!.Value))
                .ReturnsAsync(new List<RoleEo> { new RoleEo { Code = "ADMIN", Name = "Admin" } });
            var mockRefreshService = new Mock<IRefreshTokenService>();
            mockRefreshService.Setup(s => s.GenerateAsync(user.Id!.Value, true)).ReturnsAsync("refresh_token_value");
            var httpContext = new DefaultHttpContext();
            var controller = CreateController(
                authService: mockAuthService.Object,
                jwtTokenService: mockJwtService.Object,
                refreshTokenService: mockRefreshService.Object,
                roleService: mockRoleService.Object,
                options: new JwtAuthOptions { ExpiryInMinutes = 60 });
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var request = new LoginRequest { Email = "john@doe.com", Password = "password", ApplicationCode = "APP1", RememberMe = true };

            await controller.LoginAsync(request);

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
            var mockJwtService = new Mock<IJwtTokenService>();
            mockJwtService.Setup(s => s.ValidateToken("invalid_token")).Returns(new Microsoft.IdentityModel.Tokens.TokenValidationResult { IsValid = false });
            var controller = CreateController(jwtTokenService: mockJwtService.Object);
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
            var mockJwtService = new Mock<IJwtTokenService>();
            mockJwtService.Setup(s => s.ValidateToken("valid_token")).Returns(new Microsoft.IdentityModel.Tokens.TokenValidationResult { IsValid = true });
            var controller = CreateController(jwtTokenService: mockJwtService.Object);
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Authorization = "Bearer valid_token";
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = controller.Check();

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public void Check_ReturnsBadRequest_WhenExceptionThrown()
        {
            var mockJwtService = new Mock<IJwtTokenService>();
            mockJwtService.Setup(s => s.ValidateToken(It.IsAny<string>())).Throws(new Exception("Database error"));
            var controller = CreateController(jwtTokenService: mockJwtService.Object);
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
            var mockJwtService = new Mock<IJwtTokenService>();
            mockJwtService.Setup(s => s.ValidateToken("invalid_cookie_token")).Returns(new Microsoft.IdentityModel.Tokens.TokenValidationResult { IsValid = false });
            var controller = CreateController(jwtTokenService: mockJwtService.Object);
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
            var mockJwtService = new Mock<IJwtTokenService>();
            mockJwtService.Setup(s => s.ValidateToken("valid_cookie_token")).Returns(new Microsoft.IdentityModel.Tokens.TokenValidationResult { IsValid = true });
            var controller = CreateController(jwtTokenService: mockJwtService.Object);
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
            var mockJwtService = new Mock<IJwtTokenService>();
            mockJwtService.Setup(s => s.ValidateToken("invalid_token")).Returns(new Microsoft.IdentityModel.Tokens.TokenValidationResult { IsValid = false });
            var controller = CreateController(jwtTokenService: mockJwtService.Object);
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
            var mockJwtService = new Mock<IJwtTokenService>();
            mockJwtService.Setup(s => s.ValidateToken("valid_token")).Returns(new TokenValidationResult { IsValid = true, SecurityToken = null! });
            var controller = CreateController(jwtTokenService: mockJwtService.Object);
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Cookie = "auth_token=valid_token";
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = controller.Session();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<SessionResponse>(okResult.Value);
        }

        [Fact]
        public void Session_ReturnsOkWithClaimsData_WhenTokenHasClaims()
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Email, "john@doe.com"),
                new Claim(ClaimTypes.GivenName, "John"),
                new Claim(ClaimTypes.Surname, "Doe")
            };
            var jwtToken = new JwtSecurityToken(claims: claims);
            var mockJwtService = new Mock<IJwtTokenService>();
            mockJwtService.Setup(s => s.ValidateToken("claims_token")).Returns(new TokenValidationResult { IsValid = true, SecurityToken = jwtToken });
            var controller = CreateController(jwtTokenService: mockJwtService.Object);
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Cookie = "auth_token=claims_token";
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = controller.Session();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<SessionResponse>(okResult.Value);
            Assert.Equal("john@doe.com", response.Email);
            Assert.Equal("John", response.FirstName);
            Assert.Equal("Doe", response.LastName);
        }

        [Fact]
        public void Session_ReturnsOkWithEmptyData_WhenTokenHasNoClaims()
        {
            var jwtToken = new JwtSecurityToken();
            var mockJwtService = new Mock<IJwtTokenService>();
            mockJwtService.Setup(s => s.ValidateToken("no_claims_token")).Returns(new TokenValidationResult { IsValid = true, SecurityToken = jwtToken });
            var controller = CreateController(jwtTokenService: mockJwtService.Object);
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Cookie = "auth_token=no_claims_token";
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = controller.Session();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<SessionResponse>(okResult.Value);
            Assert.Equal(string.Empty, response.Email);
            Assert.Null(response.FirstName);
            Assert.Null(response.LastName);
        }

        [Fact]
        public void Session_ReturnsBadRequest_WhenExceptionThrown()
        {
            var mockJwtService = new Mock<IJwtTokenService>();
            mockJwtService.Setup(s => s.ValidateToken(It.IsAny<string>())).Throws(new Exception("Unexpected error"));
            var controller = CreateController(jwtTokenService: mockJwtService.Object);
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
            var mockJwtService = new Mock<IJwtTokenService>();
            mockJwtService.Setup(s => s.GenerateJwtToken(1, user.Email, user.FirstName, user.LastName)).Returns("new_access_token");
            var httpContext = new DefaultHttpContext();
            var controller = CreateController(
                jwtTokenService: mockJwtService.Object,
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
        public async Task RefreshAsync_ReturnsUnauthorized_WhenUserNotFound()
        {
            var mockRefreshService = new Mock<IRefreshTokenService>();
            mockRefreshService.Setup(s => s.ValidateAsync("valid_refresh")).ReturnsAsync((true, 99));
            var mockUserRepo = new Mock<IUserRepository>();
            mockUserRepo.Setup(r => r.GetUserByIdAsync(99)).ReturnsAsync((User?)null);
            var controller = CreateController(refreshTokenService: mockRefreshService.Object, userRepository: mockUserRepo.Object);
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Cookie = "refresh_token=valid_refresh";
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = await controller.RefreshAsync();

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("USER_NOT_FOUND", response.Message);
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

        [Fact]
        public async Task Logout_RevokesRefreshToken_WhenCookiePresent()
        {
            var mockRefreshService = new Mock<IRefreshTokenService>();
            mockRefreshService.Setup(s => s.RevokeAsync("existing_refresh")).Returns(Task.CompletedTask);
            var controller = CreateController(refreshTokenService: mockRefreshService.Object);
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Cookie = "refresh_token=existing_refresh";
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = await controller.Logout();

            Assert.IsType<OkResult>(result);
            mockRefreshService.Verify(s => s.RevokeAsync("existing_refresh"), Times.Once);
        }

        #endregion
    }
}
