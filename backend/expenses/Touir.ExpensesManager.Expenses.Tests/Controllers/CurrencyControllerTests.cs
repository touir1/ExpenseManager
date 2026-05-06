using Touir.ExpensesManager.Expenses.Controllers;
using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Responses;
using Touir.ExpensesManager.Expenses.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Touir.ExpensesManager.Expenses.Tests.Controllers
{
    public class CurrencyControllerTests
    {
        private static CurrencyController CreateController(ICurrencyService? service = null)
        {
            return new CurrencyController(service ?? Mock.Of<ICurrencyService>());
        }

        [Fact]
        public async Task GetAllAsync_ReturnsOk_WithCurrencies()
        {
            var currencies = new List<CurrencyDto>
            {
                new() { Id = 1, Code = "USD", Name = "US Dollar", Symbol = "$", Decimals = 2 },
                new() { Id = 2, Code = "EUR", Name = "Euro", Symbol = "€", Decimals = 2 }
            };
            var mockService = new Mock<ICurrencyService>();
            mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(currencies);
            var controller = CreateController(mockService.Object);

            var result = await controller.GetAllAsync();

            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsAssignableFrom<IEnumerable<CurrencyDto>>(ok.Value);
            Assert.Equal(2, returned.Count());
        }

        [Fact]
        public async Task GetAllAsync_ReturnsOk_WithEmptyList()
        {
            var mockService = new Mock<ICurrencyService>();
            mockService.Setup(s => s.GetAllAsync()).ReturnsAsync([]);
            var controller = CreateController(mockService.Object);

            var result = await controller.GetAllAsync();

            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsAssignableFrom<IEnumerable<CurrencyDto>>(ok.Value);
            Assert.Empty(returned);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsBadRequest_WhenServiceThrows()
        {
            var mockService = new Mock<ICurrencyService>();
            mockService.Setup(s => s.GetAllAsync()).ThrowsAsync(new Exception("db error"));
            var controller = CreateController(mockService.Object);

            var result = await controller.GetAllAsync();

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var error = Assert.IsType<ErrorResponse>(bad.Value);
            Assert.Equal("SERVER_ERROR", error.Message);
        }

        [Fact]
        public async Task GetAllAsync_CallsServiceOnce()
        {
            var mockService = new Mock<ICurrencyService>();
            mockService.Setup(s => s.GetAllAsync()).ReturnsAsync([]);
            var controller = CreateController(mockService.Object);

            await controller.GetAllAsync();

            mockService.Verify(s => s.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_MapsAllFields()
        {
            var currencies = new List<CurrencyDto>
            {
                new() { Id = 5, Code = "TND", Name = "Tunisian Dinar", Symbol = "د.ت", Decimals = 3 }
            };
            var mockService = new Mock<ICurrencyService>();
            mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(currencies);
            var controller = CreateController(mockService.Object);

            var result = await controller.GetAllAsync();

            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsAssignableFrom<IEnumerable<CurrencyDto>>(ok.Value).First();
            Assert.Equal(5, returned.Id);
            Assert.Equal("TND", returned.Code);
            Assert.Equal("Tunisian Dinar", returned.Name);
            Assert.Equal("د.ت", returned.Symbol);
            Assert.Equal(3, returned.Decimals);
        }
    }
}
