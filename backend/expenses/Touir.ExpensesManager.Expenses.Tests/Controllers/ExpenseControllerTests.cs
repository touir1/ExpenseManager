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
    public class ExpenseControllerTests
    {
        // JWT with sub=42, signed with dummy secret (nginx validates; controller just decodes)
        // Header: {"alg":"HS256","typ":"JWT"}
        // Payload: {"sub":"42","exp":9999999999}
        private const string FakeJwt =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9" +
            ".eyJzdWIiOiI0MiIsImV4cCI6OTk5OTk5OTk5OX0" +
            ".placeholder";

        private static ExpenseController CreateController(
            IExpenseService? service = null,
            string? jwtCookie = FakeJwt)
        {
            var controller = new ExpenseController(service ?? Mock.Of<IExpenseService>());
            var httpContext = new DefaultHttpContext();
            if (jwtCookie is not null)
                httpContext.Request.Headers.Cookie = $"auth_token={jwtCookie}";
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            return controller;
        }

        private static ExpenseDto MakeDto(long id = 1) => new()
        {
            Id = id,
            Amount = 100m,
            Currency = new CurrencyDto { Id = 1, Code = "USD", Name = "Dollar", Symbol = "$", Decimals = 2 },
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            CreatedAt = DateTime.UtcNow
        };

        // ── CreateAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_Returns401_WhenNoCookie()
        {
            var controller = CreateController(jwtCookie: null);
            var result = await controller.CreateAsync(new CreateExpenseRequest { Amount = 10m, CurrencyId = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow) });
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task CreateAsync_Returns201_OnSuccess()
        {
            var dto = MakeDto();
            var service = new Mock<IExpenseService>();
            service.Setup(s => s.AddAsync(It.IsAny<CreateExpenseRequest>(), It.IsAny<int>(), It.IsAny<int>()))
                   .ReturnsAsync(dto);

            var result = await CreateController(service.Object).CreateAsync(
                new CreateExpenseRequest { Amount = 50m, CurrencyId = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow) });

            Assert.IsType<CreatedAtRouteResult>(result);
        }

        [Fact]
        public async Task CreateAsync_Returns400_OnException()
        {
            var service = new Mock<IExpenseService>();
            service.Setup(s => s.AddAsync(It.IsAny<CreateExpenseRequest>(), It.IsAny<int>(), It.IsAny<int>()))
                   .ThrowsAsync(new Exception("db error"));

            var result = await CreateController(service.Object).CreateAsync(
                new CreateExpenseRequest { Amount = 50m, CurrencyId = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow) });

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var err = Assert.IsType<ErrorResponse>(bad.Value);
            Assert.Equal("SERVER_ERROR", err.Message);
        }

        [Fact]
        public async Task CreateAsync_Returns403_WhenNotFamilyMember()
        {
            var service = new Mock<IExpenseService>();
            service.Setup(s => s.AddAsync(It.IsAny<CreateExpenseRequest>(), It.IsAny<int>(), It.IsAny<int>()))
                   .ThrowsAsync(new FamilyForbiddenException());

            var result = await CreateController(service.Object).CreateAsync(
                new CreateExpenseRequest { Amount = 50m, CurrencyId = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow) });

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, obj.StatusCode);
            var err = Assert.IsType<ErrorResponse>(obj.Value);
            Assert.Equal("FAMILY_FORBIDDEN", err.Message);
        }

        // ── UpdateAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_Returns401_WhenNoCookie()
        {
            var result = await CreateController(jwtCookie: null).UpdateAsync(
                1, new UpdateExpenseRequest { Amount = 10m, CurrencyId = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow) });
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task UpdateAsync_Returns404_WhenNotFound()
        {
            var service = new Mock<IExpenseService>();
            service.Setup(s => s.UpdateAsync(It.IsAny<long>(), It.IsAny<UpdateExpenseRequest>(), It.IsAny<int>(), It.IsAny<int>()))
                   .ReturnsAsync((ExpenseDto?)null);

            var result = await CreateController(service.Object).UpdateAsync(
                1, new UpdateExpenseRequest { Amount = 10m, CurrencyId = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow) });

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task UpdateAsync_Returns200_OnSuccess()
        {
            var dto = MakeDto();
            var service = new Mock<IExpenseService>();
            service.Setup(s => s.UpdateAsync(1, It.IsAny<UpdateExpenseRequest>(), It.IsAny<int>(), It.IsAny<int>()))
                   .ReturnsAsync(dto);

            var result = await CreateController(service.Object).UpdateAsync(
                1, new UpdateExpenseRequest { Amount = 200m, CurrencyId = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow) });

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<ExpenseDto>(ok.Value);
        }

        [Fact]
        public async Task UpdateAsync_Returns403_WhenNotFamilyMember()
        {
            var service = new Mock<IExpenseService>();
            service.Setup(s => s.UpdateAsync(It.IsAny<long>(), It.IsAny<UpdateExpenseRequest>(), It.IsAny<int>(), It.IsAny<int>()))
                   .ThrowsAsync(new FamilyForbiddenException());

            var result = await CreateController(service.Object).UpdateAsync(
                1, new UpdateExpenseRequest { Amount = 50m, CurrencyId = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow) });

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, obj.StatusCode);
            var err = Assert.IsType<ErrorResponse>(obj.Value);
            Assert.Equal("FAMILY_FORBIDDEN", err.Message);
        }

        // ── DeleteAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteAsync_Returns401_WhenNoCookie()
        {
            var result = await CreateController(jwtCookie: null).DeleteAsync(1);
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task DeleteAsync_Returns404_WhenNotFound()
        {
            var service = new Mock<IExpenseService>();
            service.Setup(s => s.DeleteAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>()))
                   .ReturnsAsync(false);

            var result = await CreateController(service.Object).DeleteAsync(1);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task DeleteAsync_Returns204_OnSuccess()
        {
            var service = new Mock<IExpenseService>();
            service.Setup(s => s.DeleteAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>()))
                   .ReturnsAsync(true);

            var result = await CreateController(service.Object).DeleteAsync(1);

            Assert.IsType<NoContentResult>(result);
        }

        // ── GetByIdAsync ─────────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_Returns401_WhenNoCookie()
        {
            var result = await CreateController(jwtCookie: null).GetByIdAsync(1);
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task GetByIdAsync_Returns404_WhenNotFound()
        {
            var service = new Mock<IExpenseService>();
            service.Setup(s => s.GetByIdAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int?>()))
                   .ReturnsAsync((ExpenseDto?)null);

            var result = await CreateController(service.Object).GetByIdAsync(1);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task GetByIdAsync_Returns200_WithDto()
        {
            var dto = MakeDto(id: 5);
            var service = new Mock<IExpenseService>();
            service.Setup(s => s.GetByIdAsync(5, It.IsAny<int>(), It.IsAny<int?>())).ReturnsAsync(dto);

            var result = await CreateController(service.Object).GetByIdAsync(5);

            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<ExpenseDto>(ok.Value);
            Assert.Equal(5, returned.Id);
        }

        // ── GetPagedAsync ────────────────────────────────────────────────────────

        [Fact]
        public async Task GetPagedAsync_Returns401_WhenNoCookie()
        {
            var result = await CreateController(jwtCookie: null).GetPagedAsync(new ExpenseFilterDto());
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task GetPagedAsync_Returns200_WithPagedResponse()
        {
            var service = new Mock<IExpenseService>();
            service.Setup(s => s.GetPagedAsync(It.IsAny<ExpenseFilterDto>(), It.IsAny<int>()))
                   .ReturnsAsync(new ExpensePagedResult
                   {
                       Items = [MakeDto(1), MakeDto(2)],
                       TotalCount = 2,
                       Page = 1,
                       PageSize = 20,
                       TotalPages = 1
                   });

            var result = await CreateController(service.Object).GetPagedAsync(new ExpenseFilterDto());

            var ok = Assert.IsType<OkObjectResult>(result);
            var paged = Assert.IsType<ExpensePagedResponse>(ok.Value);
            Assert.Equal(2, paged.TotalCount);
        }

        // ── Exception fallback coverage ───────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_ServiceThrows_ReturnsBadRequest()
        {
            var service = new Mock<IExpenseService>();
            service.Setup(s => s.UpdateAsync(It.IsAny<long>(), It.IsAny<UpdateExpenseRequest>(), It.IsAny<int>(), It.IsAny<int>()))
                   .ThrowsAsync(new Exception("db"));
            var result = await CreateController(service.Object).UpdateAsync(
                1, new UpdateExpenseRequest { Amount = 10m, CurrencyId = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow) });
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task DeleteAsync_ServiceThrows_ReturnsBadRequest()
        {
            var service = new Mock<IExpenseService>();
            service.Setup(s => s.DeleteAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int>()))
                   .ThrowsAsync(new Exception("db"));
            var result = await CreateController(service.Object).DeleteAsync(1);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetByIdAsync_ServiceThrows_ReturnsBadRequest()
        {
            var service = new Mock<IExpenseService>();
            service.Setup(s => s.GetByIdAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<int?>()))
                   .ThrowsAsync(new Exception("db"));
            var result = await CreateController(service.Object).GetByIdAsync(1);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetPagedAsync_ServiceThrows_ReturnsBadRequest()
        {
            var service = new Mock<IExpenseService>();
            service.Setup(s => s.GetPagedAsync(It.IsAny<ExpenseFilterDto>(), It.IsAny<int>()))
                   .ThrowsAsync(new Exception("db"));
            var result = await CreateController(service.Object).GetPagedAsync(new ExpenseFilterDto());
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
