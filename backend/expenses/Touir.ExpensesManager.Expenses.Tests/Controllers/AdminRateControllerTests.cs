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
    public class AdminRateControllerTests
    {
        // sub=1
        private const string UserJwt =
            "eyJhbGciOiJIUzI1NiJ9" +
            ".eyJzdWIiOiIxIn0" +
            ".sig";

        private static AdminRateController CreateController(
            ICurrencyRateService? svc = null,
            string? cookieJwt = null)
        {
            var controller = new AdminRateController(svc ?? Mock.Of<ICurrencyRateService>());
            var ctx = new DefaultHttpContext();
            if (cookieJwt is not null)
                ctx.Request.Headers.Cookie = $"auth_token={cookieJwt}";
            controller.ControllerContext = new ControllerContext { HttpContext = ctx };
            return controller;
        }

        private static RateDto MakeRate() => new()
        {
            Id = 1, SourceCurrencyId = 1, DestinationCurrencyId = 2,
            Date = new DateOnly(2024, 1, 1), Rate = 1.1m, RateSource = "Manual"
        };

        [Fact]
        public async Task GetHistoryAsync_ReturnsOk_WithRates()
        {
            var svc = new Mock<ICurrencyRateService>();
            var paged = new PagedRatesResponse { Rates = [MakeRate()], Total = 1, Page = 1, PageSize = 50 };
            svc.Setup(s => s.GetRateHistoryAsync(1, 2, 1, 50)).ReturnsAsync(paged);

            var result = await CreateController(svc.Object).GetHistoryAsync(1, 2, 1, 50);

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PagedRatesResponse>(ok.Value);
            Assert.Single(response.Rates);
        }

        [Fact]
        public async Task GetHistoryAsync_ReturnsBadRequest_WhenServiceThrows()
        {
            var svc = new Mock<ICurrencyRateService>();
            svc.Setup(s => s.GetRateHistoryAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<int>()))
               .ThrowsAsync(new Exception("db"));

            var result = await CreateController(svc.Object).GetHistoryAsync(1, 2, 1, 50);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task AddRateAsync_ReturnsUnauthorized_WhenNoCookie()
        {
            var result = await CreateController().AddRateAsync(new AddRateRequest
            {
                SourceCurrencyId = 1, DestinationCurrencyId = 2,
                Date = new DateOnly(2024, 1, 1), Rate = 1.1m
            });

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task AddRateAsync_ReturnsCreated_WhenSuccessful()
        {
            var rate = MakeRate();
            var svc = new Mock<ICurrencyRateService>();
            svc.Setup(s => s.AddManualRateAsync(It.IsAny<AddRateRequest>(), 1)).ReturnsAsync(rate);

            var result = await CreateController(svc.Object, UserJwt).AddRateAsync(new AddRateRequest
            {
                SourceCurrencyId = 1, DestinationCurrencyId = 2,
                Date = new DateOnly(2024, 1, 1), Rate = 1.1m
            });

            Assert.IsType<CreatedAtRouteResult>(result);
        }

        [Fact]
        public async Task BulkAddRatesAsync_ReturnsUnauthorized_WhenNoCookie()
        {
            var result = await CreateController().BulkAddRatesAsync(new BulkAddRatesRequest { Rates = [] });

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task BulkAddRatesAsync_ReturnsNoContent_WhenSuccessful()
        {
            var svc = new Mock<ICurrencyRateService>();
            svc.Setup(s => s.BulkAddManualRatesAsync(It.IsAny<BulkAddRatesRequest>(), 1)).Returns(Task.CompletedTask);

            var result = await CreateController(svc.Object, UserJwt)
                .BulkAddRatesAsync(new BulkAddRatesRequest { Rates = [] });

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task GetPendingConflictsAsync_ReturnsOk_WithConflicts()
        {
            var conflicts = new List<RateConflictDto> { new() { Id = 1, Status = "Pending" } };
            var svc = new Mock<ICurrencyRateService>();
            svc.Setup(s => s.GetPendingConflictsAsync()).ReturnsAsync(conflicts);

            var result = await CreateController(svc.Object).GetPendingConflictsAsync();

            var ok = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsAssignableFrom<IEnumerable<RateConflictDto>>(ok.Value);
            Assert.Single(list);
        }

        [Fact]
        public async Task ResolveConflictAsync_ReturnsUnauthorized_WhenNoCookie()
        {
            var result = await CreateController()
                .ResolveConflictAsync(1, new ResolveConflictRequest { Resolution = "AcceptAuto" });

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task ResolveConflictAsync_ReturnsNoContent_WhenSuccessful()
        {
            var svc = new Mock<ICurrencyRateService>();
            svc.Setup(s => s.ResolveConflictAsync(1, It.IsAny<ResolveConflictRequest>(), 1)).Returns(Task.CompletedTask);

            var result = await CreateController(svc.Object, UserJwt)
                .ResolveConflictAsync(1, new ResolveConflictRequest { Resolution = "AcceptAuto" });

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task ResolveConflictAsync_ReturnsNotFound_WhenConflictMissing()
        {
            var svc = new Mock<ICurrencyRateService>();
            svc.Setup(s => s.ResolveConflictAsync(99, It.IsAny<ResolveConflictRequest>(), 1))
               .ThrowsAsync(new KeyNotFoundException());

            var result = await CreateController(svc.Object, UserJwt)
                .ResolveConflictAsync(99, new ResolveConflictRequest { Resolution = "AcceptAuto" });

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task RefreshRatesAsync_ReturnsNoContent_WhenSuccessful()
        {
            var svc = new Mock<ICurrencyRateService>();
            svc.Setup(s => s.RefreshRatesFromAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateController(svc.Object, UserJwt)
                .RefreshRatesAsync(new RefreshRatesRequest { From = new DateOnly(2024, 1, 1) });

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task GetHistoryAsync_OptionalParams_ReturnsOk_WithNullFilters()
        {
            var svc = new Mock<ICurrencyRateService>();
            var paged = new PagedRatesResponse { Rates = [], Total = 0, Page = 1, PageSize = 50 };
            svc.Setup(s => s.GetRateHistoryAsync(null, null, 1, 50)).ReturnsAsync(paged);

            var result = await CreateController(svc.Object).GetHistoryAsync(null, null, 1, 50);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task RefreshRatesAsync_PassesToDate_WhenProvided()
        {
            var svc = new Mock<ICurrencyRateService>();
            var toDate = new DateOnly(2024, 12, 31);
            svc.Setup(s => s.RefreshRatesFromAsync(
                It.IsAny<DateOnly>(), toDate, It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

            var result = await CreateController(svc.Object, UserJwt)
                .RefreshRatesAsync(new RefreshRatesRequest { From = new DateOnly(2024, 1, 1), To = toDate });

            Assert.IsType<NoContentResult>(result);
            svc.Verify(s => s.RefreshRatesFromAsync(
                It.IsAny<DateOnly>(), toDate, It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
