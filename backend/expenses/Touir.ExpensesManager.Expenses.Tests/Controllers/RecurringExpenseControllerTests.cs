using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Touir.ExpensesManager.Expenses.Controllers;
using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Responses;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Tests.Controllers
{
    public class RecurringExpenseControllerTests
    {
        // JWT sub=42
        private const string FakeJwt =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9" +
            ".eyJzdWIiOiI0MiIsImV4cCI6OTk5OTk5OTk5OX0" +
            ".placeholder";

        private static RecurringExpenseController CreateController(
            IRecurringExpenseService? service = null,
            string? jwtCookie = FakeJwt)
        {
            var controller = new RecurringExpenseController(service ?? Mock.Of<IRecurringExpenseService>());
            var httpContext = new DefaultHttpContext();
            if (jwtCookie is not null)
                httpContext.Request.Headers.Cookie = $"auth_token={jwtCookie}";
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            return controller;
        }

        private static RecurringExpenseDto MakeDto(int id = 1, string description = "Netflix") => new()
        {
            Id = id,
            Description = description,
            Amount = 15.99m,
            NextDueDate = new DateOnly(2026, 9, 1),
            Frequency = "Monthly"
        };

        // ── GetUpcomingAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task GetUpcoming_Returns401_WhenNoCookie()
        {
            var result = await CreateController(jwtCookie: null).GetUpcomingAsync(5);

            var response = Assert.IsType<UnauthorizedObjectResult>(result);
            var error = Assert.IsType<ErrorResponse>(response.Value);
            Assert.Equal("UNAUTHORIZED", error.Message);
        }

        [Fact]
        public async Task GetUpcoming_Returns200_WithList()
        {
            var service = new Mock<IRecurringExpenseService>();
            service.Setup(s => s.GetUpcomingAsync(42, 5)).ReturnsAsync([MakeDto(1, "Netflix"), MakeDto(2, "Rent")]);

            var result = await CreateController(service.Object).GetUpcomingAsync(5);

            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsAssignableFrom<IEnumerable<RecurringExpenseDto>>(ok.Value).ToList();
            Assert.Equal(2, returned.Count);
        }

        [Fact]
        public async Task GetUpcoming_Returns200_WithEmptyList()
        {
            var service = new Mock<IRecurringExpenseService>();
            service.Setup(s => s.GetUpcomingAsync(42, 5)).ReturnsAsync([]);

            var result = await CreateController(service.Object).GetUpcomingAsync(5);

            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsAssignableFrom<IEnumerable<RecurringExpenseDto>>(ok.Value).ToList();
            Assert.Empty(returned);
        }

        [Fact]
        public async Task GetUpcoming_UsesDefaultTake_WhenNotProvided()
        {
            var service = new Mock<IRecurringExpenseService>();
            service.Setup(s => s.GetUpcomingAsync(42, 5)).ReturnsAsync([]);

            await CreateController(service.Object).GetUpcomingAsync();

            service.Verify(s => s.GetUpcomingAsync(42, 5), Times.Once);
        }

        [Fact]
        public async Task GetUpcoming_ClampsTakeToMax20()
        {
            var service = new Mock<IRecurringExpenseService>();
            service.Setup(s => s.GetUpcomingAsync(42, 20)).ReturnsAsync([]);

            await CreateController(service.Object).GetUpcomingAsync(999);

            service.Verify(s => s.GetUpcomingAsync(42, 20), Times.Once);
        }

        [Fact]
        public async Task GetUpcoming_ClampsTakeToMin1()
        {
            var service = new Mock<IRecurringExpenseService>();
            service.Setup(s => s.GetUpcomingAsync(42, 1)).ReturnsAsync([]);

            await CreateController(service.Object).GetUpcomingAsync(0);

            service.Verify(s => s.GetUpcomingAsync(42, 1), Times.Once);
        }
    }
}
