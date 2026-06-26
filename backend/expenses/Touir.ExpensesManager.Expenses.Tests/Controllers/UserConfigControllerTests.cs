using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Touir.ExpensesManager.Expenses.Controllers;
using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Requests;
using Touir.ExpensesManager.Expenses.Controllers.Responses;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Tests.Controllers
{
    public class UserConfigControllerTests
    {
        // JWT with sub=42
        private const string FakeJwt =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9" +
            ".eyJzdWIiOiI0MiIsImV4cCI6OTk5OTk5OTk5OX0" +
            ".placeholder";

        private static UserConfigController CreateController(
            IUserConfigService? service = null,
            string? jwtCookie = FakeJwt)
        {
            var controller = new UserConfigController(service ?? Mock.Of<IUserConfigService>());
            var httpContext = new DefaultHttpContext();
            if (jwtCookie is not null)
                httpContext.Request.Headers.Cookie = $"auth_token={jwtCookie}";
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            return controller;
        }

        private static UserConfigDto MakeDto(int? currencyId = null) => new()
        {
            DefaultCurrencyId = currencyId,
            DefaultCurrency = null
        };

        // ── GetAsync ──────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAsync_Returns401_WhenNoCookie()
        {
            var controller = CreateController(jwtCookie: null);
            var result = await controller.GetAsync();
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task GetAsync_Returns200_WithNullFields_WhenNoConfigExists()
        {
            var service = new Mock<IUserConfigService>();
            service.Setup(s => s.GetAsync(42)).ReturnsAsync(MakeDto());

            var result = await CreateController(service.Object).GetAsync();

            var ok = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<UserConfigDto>(ok.Value);
            Assert.Null(dto.DefaultCurrencyId);
            Assert.Null(dto.DefaultCurrency);
        }

        [Fact]
        public async Task GetAsync_Returns200_WithPopulatedFields_WhenConfigExists()
        {
            var service = new Mock<IUserConfigService>();
            service.Setup(s => s.GetAsync(42)).ReturnsAsync(new UserConfigDto
            {
                DefaultCurrencyId = 5,
                DefaultCurrency = new CurrencyDto { Id = 5, Code = "EUR", Name = "Euro", Symbol = "€", Decimals = 2 }
            });

            var result = await CreateController(service.Object).GetAsync();

            var ok = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<UserConfigDto>(ok.Value);
            Assert.Equal(5, dto.DefaultCurrencyId);
            Assert.NotNull(dto.DefaultCurrency);
        }

        [Fact]
        public async Task GetAsync_CallsServiceWithCorrectUserId()
        {
            var service = new Mock<IUserConfigService>();
            service.Setup(s => s.GetAsync(It.IsAny<int>())).ReturnsAsync(MakeDto());

            await CreateController(service.Object).GetAsync();

            service.Verify(s => s.GetAsync(42), Times.Once);
        }

        // ── UpdateAsync ───────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_Returns401_WhenNoCookie()
        {
            var controller = CreateController(jwtCookie: null);
            var result = await controller.UpdateAsync(new UpdateUserConfigRequest { DefaultCurrencyId = 1 });
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task UpdateAsync_Returns200_WithUpdatedDto_WhenValid()
        {
            var service = new Mock<IUserConfigService>();
            service.Setup(s => s.UpdateAsync(42, 3, null)).ReturnsAsync(new UserConfigDto
            {
                DefaultCurrencyId = 3,
                DefaultCurrency = new CurrencyDto { Id = 3, Code = "USD", Name = "US Dollar", Symbol = "$", Decimals = 2 }
            });

            var result = await CreateController(service.Object)
                .UpdateAsync(new UpdateUserConfigRequest { DefaultCurrencyId = 3 });

            var ok = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<UserConfigDto>(ok.Value);
            Assert.Equal(3, dto.DefaultCurrencyId);
        }

        [Fact]
        public async Task UpdateAsync_Returns400_WhenServiceReturnsNull()
        {
            var service = new Mock<IUserConfigService>();
            service.Setup(s => s.UpdateAsync(42, 999, null)).ReturnsAsync((UserConfigDto?)null);

            var result = await CreateController(service.Object)
                .UpdateAsync(new UpdateUserConfigRequest { DefaultCurrencyId = 999 });

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var err = Assert.IsType<ErrorResponse>(bad.Value);
            Assert.Equal("USER_CONFIG_INVALID_CURRENCY", err.Message);
        }

        [Fact]
        public async Task UpdateAsync_Returns200_WhenClearingCurrencyWithNull()
        {
            var service = new Mock<IUserConfigService>();
            service.Setup(s => s.UpdateAsync(42, null, null)).ReturnsAsync(MakeDto());

            var result = await CreateController(service.Object)
                .UpdateAsync(new UpdateUserConfigRequest { DefaultCurrencyId = null });

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateAsync_CallsServiceWithCorrectArgs()
        {
            var service = new Mock<IUserConfigService>();
            service.Setup(s => s.UpdateAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>())).ReturnsAsync(MakeDto());

            await CreateController(service.Object)
                .UpdateAsync(new UpdateUserConfigRequest { DefaultCurrencyId = 7 });

            service.Verify(s => s.UpdateAsync(42, 7, null), Times.Once);
        }
    }
}
