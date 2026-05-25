using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Touir.ExpensesManager.Expenses.Controllers;
using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Responses;
using Touir.ExpensesManager.Expenses.Services;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Tests.Controllers
{
    public class DashboardControllerTests
    {
        // JWT with sub=42
        private const string FakeJwt =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9" +
            ".eyJzdWIiOiI0MiIsImV4cCI6OTk5OTk5OTk5OX0" +
            ".placeholder";

        private static DashboardController CreateController(
            IDashboardService? service = null,
            string? jwtCookie = FakeJwt)
        {
            var controller = new DashboardController(service ?? Mock.Of<IDashboardService>());
            var httpContext = new DefaultHttpContext();
            if (jwtCookie is not null)
                httpContext.Request.Headers.Cookie = $"auth_token={jwtCookie}";
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            return controller;
        }

        private static DashboardSummaryDto MakeSummary() => new()
        {
            TotalAmount = 100m,
            ExpenseCount = 2
        };

        // ── GetSummaryAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task GetSummaryAsync_Returns401_WhenNoCookie()
        {
            var controller = CreateController(jwtCookie: null);
            var result = await controller.GetSummaryAsync(null, null, null, null);
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task GetSummaryAsync_Returns200_WithDefaultDateRange_WhenNoParamsProvided()
        {
            var service = new Mock<IDashboardService>();
            service.Setup(s => s.GetSummaryAsync(42, null, It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), null))
                   .ReturnsAsync(MakeSummary());

            var result = await CreateController(service.Object).GetSummaryAsync(null, null, null, null);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetSummaryAsync_Returns200_WithExplicitDateRange()
        {
            var from = new DateOnly(2026, 1, 1);
            var to = new DateOnly(2026, 1, 31);
            var service = new Mock<IDashboardService>();
            service.Setup(s => s.GetSummaryAsync(42, null, from, to, null)).ReturnsAsync(MakeSummary());

            var result = await CreateController(service.Object).GetSummaryAsync(null, from, to, null);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetSummaryAsync_Returns403_OnFamilyForbiddenException()
        {
            var service = new Mock<IDashboardService>();
            service.Setup(s => s.GetSummaryAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), null))
                   .ThrowsAsync(new FamilyForbiddenException());

            var result = await CreateController(service.Object).GetSummaryAsync(null, null, null, null);

            Assert.Equal(StatusCodes.Status403Forbidden, ((ObjectResult)result).StatusCode);
        }

        [Fact]
        public async Task GetSummaryAsync_Returns400_OnGenericException()
        {
            var service = new Mock<IDashboardService>();
            service.Setup(s => s.GetSummaryAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), null))
                   .ThrowsAsync(new Exception("unexpected"));

            var result = await CreateController(service.Object).GetSummaryAsync(null, null, null, null);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // ── GetMonthlyAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task GetMonthlyAsync_Returns401_WhenNoCookie()
        {
            var controller = CreateController(jwtCookie: null);
            var result = await controller.GetMonthlyAsync(null, null, null, null);
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task GetMonthlyAsync_Returns200_OnSuccess()
        {
            var service = new Mock<IDashboardService>();
            service.Setup(s => s.GetMonthlyAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), null))
                   .ReturnsAsync([]);

            var result = await CreateController(service.Object).GetMonthlyAsync(null, null, null, null);

            Assert.IsType<OkObjectResult>(result);
        }

        // ── GetCategoriesAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task GetCategoriesAsync_Returns200_OnSuccess()
        {
            var service = new Mock<IDashboardService>();
            service.Setup(s => s.GetCategoriesAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), null))
                   .ReturnsAsync([]);

            var result = await CreateController(service.Object).GetCategoriesAsync(null, null, null, null);

            Assert.IsType<OkObjectResult>(result);
        }

        // ── GetSameMonthAcrossYearsAsync ──────────────────────────────────────────

        [Fact]
        public async Task GetSameMonthAcrossYearsAsync_Returns400_WhenMonthOutOfRange()
        {
            var result = await CreateController().GetSameMonthAcrossYearsAsync(month: 13, null, null);
            Assert.IsType<BadRequestObjectResult>(result);
            var err = ((BadRequestObjectResult)result).Value as ErrorResponse;
            Assert.Equal("INVALID_MONTH", err?.Message);
        }

        [Fact]
        public async Task GetSameMonthAcrossYearsAsync_Returns400_WhenMonthZero()
        {
            var result = await CreateController().GetSameMonthAcrossYearsAsync(month: 0, null, null);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetSameMonthAcrossYearsAsync_Returns200_OnSuccess()
        {
            var service = new Mock<IDashboardService>();
            service.Setup(s => s.GetSameMonthAcrossYearsAsync(It.IsAny<int>(), It.IsAny<int?>(), 3, null))
                   .ReturnsAsync([]);

            var result = await CreateController(service.Object).GetSameMonthAcrossYearsAsync(month: 3, null, null);

            Assert.IsType<OkObjectResult>(result);
        }

        // ── GetByCurrencyAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task GetByCurrencyAsync_Returns200_OnSuccess()
        {
            var service = new Mock<IDashboardService>();
            service.Setup(s => s.GetByCurrencyAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), null))
                   .ReturnsAsync([]);

            var result = await CreateController(service.Object).GetByCurrencyAsync(null, null, null, null);

            Assert.IsType<OkObjectResult>(result);
        }

        // ── GetRecentAsync ────────────────────────────────────────────────────────

        [Fact]
        public async Task GetRecentAsync_Returns200_OnSuccess()
        {
            var service = new Mock<IDashboardService>();
            service.Setup(s => s.GetRecentAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(), It.IsAny<int?>()))
                   .ReturnsAsync(new ExpensePagedResult { Items = [], TotalCount = 0, Page = 1, PageSize = 10, TotalPages = 0 });

            var result = await CreateController(service.Object).GetRecentAsync(null, null, null, null);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetRecentAsync_Returns403_OnFamilyForbiddenException()
        {
            var service = new Mock<IDashboardService>();
            service.Setup(s => s.GetRecentAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(), It.IsAny<int?>()))
                   .ThrowsAsync(new FamilyForbiddenException());

            var result = await CreateController(service.Object).GetRecentAsync(null, null, null, null);

            Assert.Equal(StatusCodes.Status403Forbidden, ((ObjectResult)result).StatusCode);
        }
    }
}
