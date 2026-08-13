using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Touir.ExpensesManager.Expenses.Controllers;
using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Requests;
using Touir.ExpensesManager.Expenses.Controllers.Responses;
using Touir.ExpensesManager.Expenses.Services;
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

        private static CreateRecurringExpenseRequest MakeCreateRequest() => new()
        {
            Description = "Netflix",
            Amount = 15.99m,
            CurrencyId = 1,
            CategoryId = 1,
            FrequencyId = 2,
            NextDueDate = new DateOnly(2026, 9, 1)
        };

        private static UpdateRecurringExpenseRequest MakeUpdateRequest() => new()
        {
            Description = "Netflix",
            Amount = 15.99m,
            CurrencyId = 1,
            CategoryId = 1,
            FrequencyId = 2,
            NextDueDate = new DateOnly(2026, 9, 1),
            IsActive = true
        };

        // ── GetAllAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetAll_Returns401_WhenNoCookie()
        {
            var result = await CreateController(jwtCookie: null).GetAllAsync();

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task GetAll_Returns200_WithList()
        {
            var service = new Mock<IRecurringExpenseService>();
            service.Setup(s => s.GetAllAsync(42, false)).ReturnsAsync([MakeDto(1), MakeDto(2)]);

            var result = await CreateController(service.Object).GetAllAsync();

            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsAssignableFrom<IEnumerable<RecurringExpenseDto>>(ok.Value).ToList();
            Assert.Equal(2, returned.Count);
        }

        // ── GetByIdAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task GetById_Returns401_WhenNoCookie()
        {
            var result = await CreateController(jwtCookie: null).GetByIdAsync(1);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task GetById_Returns404_WhenNotFound()
        {
            var service = new Mock<IRecurringExpenseService>();
            service.Setup(s => s.GetByIdAsync(1, 42)).ReturnsAsync((RecurringExpenseDto?)null);

            var result = await CreateController(service.Object).GetByIdAsync(1);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            var error = Assert.IsType<ErrorResponse>(notFound.Value);
            Assert.Equal("RECURRING_EXPENSE_NOT_FOUND", error.Message);
        }

        [Fact]
        public async Task GetById_Returns200_WhenFound()
        {
            var service = new Mock<IRecurringExpenseService>();
            service.Setup(s => s.GetByIdAsync(1, 42)).ReturnsAsync(MakeDto(1));

            var result = await CreateController(service.Object).GetByIdAsync(1);

            Assert.IsType<OkObjectResult>(result);
        }

        // ── CreateAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task Create_Returns401_WhenNoCookie()
        {
            var result = await CreateController(jwtCookie: null).CreateAsync(MakeCreateRequest());

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Create_Returns201_WithCreatedAtRoute()
        {
            var service = new Mock<IRecurringExpenseService>();
            service.Setup(s => s.CreateAsync(It.IsAny<CreateRecurringExpenseRequest>(), 42)).ReturnsAsync(MakeDto(7));

            var result = await CreateController(service.Object).CreateAsync(MakeCreateRequest());

            var created = Assert.IsType<CreatedAtRouteResult>(result);
            Assert.Equal("GetRecurringExpenseById", created.RouteName);
            Assert.Equal(7, ((RecurringExpenseDto)created.Value!).Id);
        }

        [Fact]
        public async Task Create_Returns400_OnServiceException()
        {
            var service = new Mock<IRecurringExpenseService>();
            service.Setup(s => s.CreateAsync(It.IsAny<CreateRecurringExpenseRequest>(), 42))
                .ThrowsAsync(new InvalidOperationException());

            var result = await CreateController(service.Object).CreateAsync(MakeCreateRequest());

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // ── UpdateAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task Update_Returns401_WhenNoCookie()
        {
            var result = await CreateController(jwtCookie: null).UpdateAsync(1, MakeUpdateRequest());

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Update_Returns404_WhenNotFound()
        {
            var service = new Mock<IRecurringExpenseService>();
            service.Setup(s => s.UpdateAsync(1, It.IsAny<UpdateRecurringExpenseRequest>(), 42)).ReturnsAsync((RecurringExpenseDto?)null);

            var result = await CreateController(service.Object).UpdateAsync(1, MakeUpdateRequest());

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Update_Returns200_WhenUpdated()
        {
            var service = new Mock<IRecurringExpenseService>();
            service.Setup(s => s.UpdateAsync(1, It.IsAny<UpdateRecurringExpenseRequest>(), 42)).ReturnsAsync(MakeDto(1));

            var result = await CreateController(service.Object).UpdateAsync(1, MakeUpdateRequest());

            Assert.IsType<OkObjectResult>(result);
        }

        // ── DeleteAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task Delete_Returns401_WhenNoCookie()
        {
            var result = await CreateController(jwtCookie: null).DeleteAsync(1);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Delete_Returns404_WhenNotFound()
        {
            var service = new Mock<IRecurringExpenseService>();
            service.Setup(s => s.DeleteAsync(1, 42)).ReturnsAsync(false);

            var result = await CreateController(service.Object).DeleteAsync(1);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Delete_Returns204_WhenDeleted()
        {
            var service = new Mock<IRecurringExpenseService>();
            service.Setup(s => s.DeleteAsync(1, 42)).ReturnsAsync(true);

            var result = await CreateController(service.Object).DeleteAsync(1);

            Assert.IsType<NoContentResult>(result);
        }

        // ── ConfirmAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task Confirm_Returns401_WhenNoCookie()
        {
            var result = await CreateController(jwtCookie: null).ConfirmAsync(1);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Confirm_Returns404_WhenNotFound()
        {
            var service = new Mock<IRecurringExpenseService>();
            service.Setup(s => s.ConfirmAsync(1, 42)).ReturnsAsync((ExpenseDto?)null);

            var result = await CreateController(service.Object).ConfirmAsync(1);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            var error = Assert.IsType<ErrorResponse>(notFound.Value);
            Assert.Equal("RECURRING_EXPENSE_NOT_FOUND", error.Message);
        }

        [Fact]
        public async Task Confirm_Returns400_WhenNotDue()
        {
            var service = new Mock<IRecurringExpenseService>();
            service.Setup(s => s.ConfirmAsync(1, 42)).ThrowsAsync(new RecurringExpenseNotDueException());

            var result = await CreateController(service.Object).ConfirmAsync(1);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var error = Assert.IsType<ErrorResponse>(badRequest.Value);
            Assert.Equal("RECURRING_NOT_DUE", error.Message);
        }

        [Fact]
        public async Task Confirm_Returns200_WithExpenseDto()
        {
            var service = new Mock<IRecurringExpenseService>();
            service.Setup(s => s.ConfirmAsync(1, 42)).ReturnsAsync(new ExpenseDto { Id = 55 });

            var result = await CreateController(service.Object).ConfirmAsync(1);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(55, ((ExpenseDto)ok.Value!).Id);
        }
    }
}
