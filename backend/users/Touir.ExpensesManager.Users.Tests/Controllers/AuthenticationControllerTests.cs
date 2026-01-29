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
        public async Task ChangePasswordReset_ReturnsOk_WhenValid()
        {
            var mockAuthService = new Mock<IAuthenticationService>();
            mockAuthService.Setup(s => s.ResetPasswordAsync("john@doe.com", "hash", "new")).ReturnsAsync(true);
            var controller = new AuthenticationController(mockAuthService.Object, Mock.Of<IRoleService>(), Mock.Of<IApplicationService>());
            var request = new ChangePasswordResetRequest { Email = "john@doe.com", VerificationHash = "hash", NewPassword = "new", ConfirmPassword = "new" };
            
            var result = await controller.ChangePasswordReset(request);
            
            Assert.IsType<OkResult>(result);
        }
    }
}
