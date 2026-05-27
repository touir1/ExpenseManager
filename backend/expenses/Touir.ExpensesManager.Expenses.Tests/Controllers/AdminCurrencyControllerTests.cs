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
    public class AdminCurrencyControllerTests
    {
        private static AdminCurrencyController CreateController(
            IAdminCurrencyService? currSvc = null,
            ICurrencyRateService? rateSvc = null,
            string? cookieJwt = null)
        {
            var controller = new AdminCurrencyController(
                currSvc ?? Mock.Of<IAdminCurrencyService>(),
                rateSvc ?? Mock.Of<ICurrencyRateService>());

            var ctx = new DefaultHttpContext();
            if (cookieJwt is not null)
                ctx.Request.Headers.Cookie = $"auth_token={cookieJwt}";
            controller.ControllerContext = new ControllerContext { HttpContext = ctx };
            return controller;
        }

        // sub=1, isAdmin="true"
        private const string AdminJwt =
            "eyJhbGciOiJIUzI1NiJ9" +
            ".eyJzdWIiOiIxIiwiaXNBZG1pbiI6InRydWUifQ" +
            ".sig";

        [Fact]
        public async Task AddCurrencyAsync_Returns201_WhenSuccessful()
        {
            var dto = new CurrencyDto { Id = 1, Code = "JPY", Name = "Yen", Symbol = "¥", Decimals = 0 };
            var svc = new Mock<IAdminCurrencyService>();
            svc.Setup(s => s.AddCurrencyAsync("JPY", "Yen", "¥", 0)).ReturnsAsync(dto);

            var result = await CreateController(currSvc: svc.Object)
                .AddCurrencyAsync(new AdminAddCurrencyRequest { Code = "JPY", Name = "Yen", Symbol = "¥", Decimals = 0 });

            var status = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, status.StatusCode);
        }

        [Fact]
        public async Task AddCurrencyAsync_ReturnsConflict_WhenDuplicateCode()
        {
            var svc = new Mock<IAdminCurrencyService>();
            svc.Setup(s => s.AddCurrencyAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
               .ThrowsAsync(new InvalidOperationException("CURRENCY_CODE_ALREADY_EXISTS"));

            var result = await CreateController(currSvc: svc.Object)
                .AddCurrencyAsync(new AdminAddCurrencyRequest { Code = "USD", Name = "Dollar", Symbol = "$", Decimals = 2 });

            Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        public async Task SetDefaultRateAsync_ReturnsUnauthorized_WhenNoCookie()
        {
            var result = await CreateController()
                .SetDefaultRateAsync(new SetDefaultRateRequest { SourceCurrencyId = 1, DestinationCurrencyId = 2, Rate = 1.2m });

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task SetDefaultRateAsync_ReturnsNoContent_WhenSuccessful()
        {
            var rateSvc = new Mock<ICurrencyRateService>();
            rateSvc.Setup(s => s.SetDefaultFallbackAsync(It.IsAny<SetDefaultRateRequest>(), 1))
                   .Returns(Task.CompletedTask);

            var result = await CreateController(rateSvc: rateSvc.Object, cookieJwt: AdminJwt)
                .SetDefaultRateAsync(new SetDefaultRateRequest { SourceCurrencyId = 1, DestinationCurrencyId = 2, Rate = 1.2m });

            Assert.IsType<NoContentResult>(result);
        }
    }
}
