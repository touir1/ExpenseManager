using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Touir.ExpensesManager.Users.Controllers;
using Touir.ExpensesManager.Users.Controllers.DTO;
using Touir.ExpensesManager.Users.Controllers.Requests;
using Touir.ExpensesManager.Users.Controllers.Responses;
using Touir.ExpensesManager.Users.Services.Contracts;

namespace Touir.ExpensesManager.Users.Tests.Controllers
{
    public class AdminUserControllerTests
    {
        // sub=1 — a minimal valid JWT payload
        private const string UserJwt =
            "eyJhbGciOiJIUzI1NiJ9" +
            ".eyJzdWIiOiIxIn0" +
            ".sig";

        private static AdminUserController CreateController(
            IAdminUserService? svc = null,
            string? cookieJwt = null)
        {
            var controller = new AdminUserController(svc ?? Mock.Of<IAdminUserService>());
            var ctx = new DefaultHttpContext();
            if (cookieJwt is not null)
                ctx.Request.Headers.Cookie = $"auth_token={cookieJwt}";
            controller.ControllerContext = new ControllerContext { HttpContext = ctx };
            return controller;
        }

        private static AdminUserDto MakeDto(int id = 1, string email = "a@b.com")
            => new() { Id = id, Email = email, FirstName = "A", LastName = "B", Roles = [] };

        [Fact]
        public async Task GetUsersAsync_ReturnsOk_WithPagedResult()
        {
            var svc = new Mock<IAdminUserService>();
            svc.Setup(s => s.GetUsersPagedAsync(null, 1, 20))
               .ReturnsAsync(([MakeDto(), MakeDto(2, "c@d.com")], 2));

            var result = await CreateController(svc.Object).GetUsersAsync(null, 1, 20);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetUsersAsync_ReturnsBadRequest_WhenServiceThrows()
        {
            var svc = new Mock<IAdminUserService>();
            svc.Setup(s => s.GetUsersPagedAsync(It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<int>()))
               .ThrowsAsync(new Exception("db"));

            var result = await CreateController(svc.Object).GetUsersAsync(null);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetRolesAsync_ReturnsOk_WithRoles()
        {
            var roles = new List<RoleDto> { new() { Id = 1, Code = "ADMIN", Name = "Admin" } };
            var svc = new Mock<IAdminUserService>();
            svc.Setup(s => s.GetAllRolesAsync()).ReturnsAsync(roles);

            var result = await CreateController(svc.Object).GetRolesAsync();

            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsAssignableFrom<IEnumerable<RoleDto>>(ok.Value);
            Assert.Single(returned);
        }

        [Fact]
        public async Task DisableUserAsync_ReturnsNoContent_WhenUserExists()
        {
            var svc = new Mock<IAdminUserService>();
            svc.Setup(s => s.DisableUserAsync(1)).ReturnsAsync(true);

            var result = await CreateController(svc.Object).DisableUserAsync(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DisableUserAsync_ReturnsNotFound_WhenUserMissing()
        {
            var svc = new Mock<IAdminUserService>();
            svc.Setup(s => s.DisableUserAsync(99)).ReturnsAsync(false);

            var result = await CreateController(svc.Object).DisableUserAsync(99);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task EnableUserAsync_ReturnsNoContent_WhenUserExists()
        {
            var svc = new Mock<IAdminUserService>();
            svc.Setup(s => s.EnableUserAsync(1)).ReturnsAsync(true);

            var result = await CreateController(svc.Object).EnableUserAsync(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task SetUserRolesAsync_ReturnsUnauthorized_WhenNoCookie()
        {
            var result = await CreateController()
                .SetUserRolesAsync(1, new SetUserRolesRequest { RoleIds = [1] });

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task SetUserRolesAsync_ReturnsNoContent_WhenSuccessful()
        {
            var svc = new Mock<IAdminUserService>();
            svc.Setup(s => s.SetUserRolesAsync(1, It.IsAny<IEnumerable<int>>(), 1)).Returns(Task.CompletedTask);

            var result = await CreateController(svc.Object, UserJwt)
                .SetUserRolesAsync(1, new SetUserRolesRequest { RoleIds = [2] });

            Assert.IsType<NoContentResult>(result);
        }
    }
}
