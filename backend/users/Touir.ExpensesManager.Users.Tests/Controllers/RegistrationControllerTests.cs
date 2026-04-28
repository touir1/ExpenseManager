using Touir.ExpensesManager.Users.Controllers;
using Touir.ExpensesManager.Users.Controllers.Requests;
using Touir.ExpensesManager.Users.Controllers.Responses;
using Touir.ExpensesManager.Users.Controllers.EO;
using Touir.ExpensesManager.Users.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Touir.ExpensesManager.Users.Tests.Controllers
{
    public class RegistrationControllerTests
    {
        private static RegistrationController CreateController(
            IRegistrationService? registrationService = null,
            IApplicationService? appService = null)
        {
            return new RegistrationController(
                registrationService ?? Mock.Of<IRegistrationService>(),
                appService ?? Mock.Of<IApplicationService>());
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
            var mockService = new Mock<IRegistrationService>();
            mockService.Setup(s => s.RegisterNewUserAsync("John", "Doe", "john@doe.com", "APP1"))
                .ReturnsAsync(new List<string>());
            var controller = CreateController(registrationService: mockService.Object);
            var request = new RegisterRequest { FirstName = "John", LastName = "Doe", Email = "john@doe.com", ApplicationCode = "APP1" };
            var result = await controller.RegisterAsync(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<RegisterResponse>(okResult.Value);
            Assert.False(response.HasError);
        }

        [Fact]
        public async Task RegisterAsync_ReturnsOkWithErrors_WhenServiceReturnsErrors()
        {
            var mockService = new Mock<IRegistrationService>();
            mockService.Setup(s => s.RegisterNewUserAsync("John", "Doe", "john@doe.com", "APP1"))
                .ReturnsAsync(new List<string> { "email is already used" });
            var controller = CreateController(registrationService: mockService.Object);
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
            var mockService = new Mock<IRegistrationService>();
            mockService.Setup(s => s.RegisterNewUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Database error"));
            var controller = CreateController(registrationService: mockService.Object);
            var request = new RegisterRequest { FirstName = "John", LastName = "Doe", Email = "john@doe.com", ApplicationCode = "APP1" };
            var result = await controller.RegisterAsync(request);

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
            var mockService = new Mock<IRegistrationService>();
            mockService.Setup(s => s.ValidateEmailAsync("hash", "john@doe.com")).ReturnsAsync(false);
            var controller = CreateController(registrationService: mockService.Object, appService: mockAppService.Object);
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
            var mockService = new Mock<IRegistrationService>();
            mockService.Setup(s => s.ValidateEmailAsync("hash", "john@doe.com")).ReturnsAsync(true);
            var controller = CreateController(registrationService: mockService.Object, appService: mockAppService.Object);

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
    }
}
