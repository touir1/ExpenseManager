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
    public class CurrencyRateControllerTests
    {
        private const string FakeJwt =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9" +
            ".eyJzdWIiOiI0MiIsImV4cCI6OTk5OTk5OTk5OX0" +
            ".placeholder";

        private static CurrencyRateController CreateController(ICurrencyRateService? service = null, string? jwt = FakeJwt)
        {
            var controller = new CurrencyRateController(service ?? Mock.Of<ICurrencyRateService>());
            var ctx = new DefaultHttpContext();
            if (jwt is not null)
                ctx.Request.Headers.Cookie = $"auth_token={jwt}";
            controller.ControllerContext = new ControllerContext { HttpContext = ctx };
            return controller;
        }

        private static RateDto MakeRateDto() => new()
        {
            Id = 1, SourceCurrencyId = 1, DestinationCurrencyId = 2,
            Date = new DateOnly(2024, 6, 1), Rate = 0.92m, RateSource = "Manual"
        };

        // ── GetHistoryAsync ────────────────────────────────────────────────────────

        [Fact]
        public async Task GetHistory_ReturnsOk_WithRates()
        {
            var service = new Mock<ICurrencyRateService>();
            service.Setup(s => s.GetRateHistoryAsync(1, 2)).ReturnsAsync([MakeRateDto()]);

            var result = await CreateController(service.Object).GetHistoryAsync(1, 2);

            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsAssignableFrom<IEnumerable<RateDto>>(ok.Value);
            Assert.Single(returned);
        }

        [Fact]
        public async Task GetHistory_ServiceThrows_ReturnsBadRequest()
        {
            var service = new Mock<ICurrencyRateService>();
            service.Setup(s => s.GetRateHistoryAsync(It.IsAny<int>(), It.IsAny<int>()))
                   .ThrowsAsync(new Exception("db error"));

            var result = await CreateController(service.Object).GetHistoryAsync(1, 2);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<ErrorResponse>(bad.Value);
        }

        // ── AddRateAsync ───────────────────────────────────────────────────────────

        [Fact]
        public async Task AddRate_MissingUser_Returns401()
        {
            var result = await CreateController(jwt: null).AddRateAsync(
                new AddRateRequest { SourceCurrencyId = 1, DestinationCurrencyId = 2, Date = new DateOnly(2024, 6, 1), Rate = 0.92m });
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task AddRate_ValidRequest_Returns201()
        {
            var service = new Mock<ICurrencyRateService>();
            service.Setup(s => s.AddManualRateAsync(It.IsAny<AddRateRequest>(), It.IsAny<int>()))
                   .ReturnsAsync(MakeRateDto());

            var result = await CreateController(service.Object).AddRateAsync(
                new AddRateRequest { SourceCurrencyId = 1, DestinationCurrencyId = 2, Date = new DateOnly(2024, 6, 1), Rate = 0.92m });

            Assert.IsType<CreatedAtRouteResult>(result);
        }

        [Fact]
        public async Task AddRate_ServiceThrows_ReturnsBadRequest()
        {
            var service = new Mock<ICurrencyRateService>();
            service.Setup(s => s.AddManualRateAsync(It.IsAny<AddRateRequest>(), It.IsAny<int>()))
                   .ThrowsAsync(new Exception("db error"));

            var result = await CreateController(service.Object).AddRateAsync(
                new AddRateRequest { SourceCurrencyId = 1, DestinationCurrencyId = 2, Date = new DateOnly(2024, 6, 1), Rate = 0.92m });

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // ── BulkAddRatesAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task BulkAddRates_MissingUser_Returns401()
        {
            var result = await CreateController(jwt: null).BulkAddRatesAsync(new BulkAddRatesRequest { Rates = [] });
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task BulkAddRates_ValidRequest_Returns204()
        {
            var service = new Mock<ICurrencyRateService>();
            service.Setup(s => s.BulkAddManualRatesAsync(It.IsAny<BulkAddRatesRequest>(), It.IsAny<int>()))
                   .Returns(Task.CompletedTask);

            var result = await CreateController(service.Object).BulkAddRatesAsync(new BulkAddRatesRequest { Rates = [] });

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task BulkAddRates_ServiceThrows_ReturnsBadRequest()
        {
            var service = new Mock<ICurrencyRateService>();
            service.Setup(s => s.BulkAddManualRatesAsync(It.IsAny<BulkAddRatesRequest>(), It.IsAny<int>()))
                   .ThrowsAsync(new Exception("db error"));

            var result = await CreateController(service.Object).BulkAddRatesAsync(new BulkAddRatesRequest { Rates = [] });

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // ── SetDefaultAsync ────────────────────────────────────────────────────────

        [Fact]
        public async Task SetDefault_MissingUser_Returns401()
        {
            var result = await CreateController(jwt: null).SetDefaultAsync(
                new SetDefaultRateRequest { SourceCurrencyId = 1, DestinationCurrencyId = 2, Rate = 0.9m });
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task SetDefault_ValidRequest_Returns204()
        {
            var service = new Mock<ICurrencyRateService>();
            service.Setup(s => s.SetDefaultFallbackAsync(It.IsAny<SetDefaultRateRequest>(), It.IsAny<int>()))
                   .Returns(Task.CompletedTask);

            var result = await CreateController(service.Object).SetDefaultAsync(
                new SetDefaultRateRequest { SourceCurrencyId = 1, DestinationCurrencyId = 2, Rate = 0.9m });

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task SetDefault_ServiceThrows_ReturnsBadRequest()
        {
            var service = new Mock<ICurrencyRateService>();
            service.Setup(s => s.SetDefaultFallbackAsync(It.IsAny<SetDefaultRateRequest>(), It.IsAny<int>()))
                   .ThrowsAsync(new Exception("db error"));

            var result = await CreateController(service.Object).SetDefaultAsync(
                new SetDefaultRateRequest { SourceCurrencyId = 1, DestinationCurrencyId = 2, Rate = 0.9m });

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // ── GetConflictsAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetConflicts_ReturnsOk_WithList()
        {
            var service = new Mock<ICurrencyRateService>();
            service.Setup(s => s.GetPendingConflictsAsync()).ReturnsAsync([
                new RateConflictDto { Id = 1, SourceCurrencyId = 1, DestinationCurrencyId = 2,
                    Date = new DateOnly(2024, 6, 1), AutomaticRate = 0.91m, ManualRate = 0.92m, Status = "Pending" }
            ]);

            var result = await CreateController(service.Object).GetConflictsAsync();

            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsAssignableFrom<IEnumerable<RateConflictDto>>(ok.Value);
            Assert.Single(returned);
        }

        // ── RefreshRatesAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task RefreshRates_MissingUser_Returns401()
        {
            var result = await CreateController(jwt: null).RefreshRatesAsync(
                new RefreshRatesRequest { From = new DateOnly(2024, 6, 1) });
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task RefreshRates_ValidRequest_Returns204()
        {
            var service = new Mock<ICurrencyRateService>();
            service.Setup(s => s.RefreshRatesFromAsync(It.IsAny<DateOnly>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                   .Returns(Task.CompletedTask);

            var result = await CreateController(service.Object).RefreshRatesAsync(
                new RefreshRatesRequest { From = new DateOnly(2024, 6, 1) });

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task RefreshRates_WithSourceAndDestFilter_PassesThrough()
        {
            var service = new Mock<ICurrencyRateService>();
            service.Setup(s => s.RefreshRatesFromAsync(It.IsAny<DateOnly>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                   .Returns(Task.CompletedTask);

            var result = await CreateController(service.Object).RefreshRatesAsync(
                new RefreshRatesRequest { From = new DateOnly(2024, 6, 1), SourceCurrencyId = 1, DestinationCurrencyId = 2 });

            Assert.IsType<NoContentResult>(result);
            service.Verify(s => s.RefreshRatesFromAsync(
                new DateOnly(2024, 6, 1), 1, 2, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RefreshRates_ServiceThrows_ReturnsBadRequest()
        {
            var service = new Mock<ICurrencyRateService>();
            service.Setup(s => s.RefreshRatesFromAsync(It.IsAny<DateOnly>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new Exception("provider error"));

            var result = await CreateController(service.Object).RefreshRatesAsync(
                new RefreshRatesRequest { From = new DateOnly(2024, 6, 1) });

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetConflicts_ServiceThrows_ReturnsBadRequest()
        {
            var service = new Mock<ICurrencyRateService>();
            service.Setup(s => s.GetPendingConflictsAsync()).ThrowsAsync(new Exception("db error"));

            var result = await CreateController(service.Object).GetConflictsAsync();

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // ── ResolveConflictAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task ResolveConflict_MissingUser_Returns401()
        {
            var result = await CreateController(jwt: null).ResolveConflictAsync(1,
                new ResolveConflictRequest { Resolution = "KeepManual" });
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task ResolveConflict_ValidRequest_Returns204()
        {
            var service = new Mock<ICurrencyRateService>();
            service.Setup(s => s.ResolveConflictAsync(It.IsAny<int>(), It.IsAny<ResolveConflictRequest>(), It.IsAny<int>()))
                   .Returns(Task.CompletedTask);

            var result = await CreateController(service.Object).ResolveConflictAsync(1,
                new ResolveConflictRequest { Resolution = "KeepManual" });

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task ResolveConflict_ServiceThrows_ReturnsBadRequest()
        {
            var service = new Mock<ICurrencyRateService>();
            service.Setup(s => s.ResolveConflictAsync(It.IsAny<int>(), It.IsAny<ResolveConflictRequest>(), It.IsAny<int>()))
                   .ThrowsAsync(new Exception("db error"));

            var result = await CreateController(service.Object).ResolveConflictAsync(1,
                new ResolveConflictRequest { Resolution = "KeepManual" });

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ResolveConflict_NotFound_Returns404()
        {
            var service = new Mock<ICurrencyRateService>();
            service.Setup(s => s.ResolveConflictAsync(It.IsAny<int>(), It.IsAny<ResolveConflictRequest>(), It.IsAny<int>()))
                   .ThrowsAsync(new KeyNotFoundException("Conflict 99 not found."));

            var result = await CreateController(service.Object).ResolveConflictAsync(99,
                new ResolveConflictRequest { Resolution = "KeepManual" });

            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
