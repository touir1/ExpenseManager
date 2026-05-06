using Touir.ExpensesManager.Expenses.Controllers;
using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Responses;
using Touir.ExpensesManager.Expenses.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Touir.ExpensesManager.Expenses.Tests.Controllers
{
    public class CategoryControllerTests
    {
        private static CategoryController CreateController(ICategoryService? service = null)
        {
            return new CategoryController(service ?? Mock.Of<ICategoryService>());
        }

        [Fact]
        public async Task GetAllAsync_ReturnsOk_WithCategories()
        {
            var categories = new List<CategoryDto>
            {
                new() { Id = 1, Name = "Food", Subcategories = [new SubcategoryDto { Id = 2, Name = "Groceries" }] },
                new() { Id = 3, Name = "Transport", Subcategories = [] }
            };
            var mockService = new Mock<ICategoryService>();
            mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(categories);
            var controller = CreateController(mockService.Object);

            var result = await controller.GetAllAsync();

            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsAssignableFrom<IEnumerable<CategoryDto>>(ok.Value);
            Assert.Equal(2, returned.Count());
        }

        [Fact]
        public async Task GetAllAsync_ReturnsOk_WithEmptyList()
        {
            var mockService = new Mock<ICategoryService>();
            mockService.Setup(s => s.GetAllAsync()).ReturnsAsync([]);
            var controller = CreateController(mockService.Object);

            var result = await controller.GetAllAsync();

            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsAssignableFrom<IEnumerable<CategoryDto>>(ok.Value);
            Assert.Empty(returned);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsBadRequest_WhenServiceThrows()
        {
            var mockService = new Mock<ICategoryService>();
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
            var mockService = new Mock<ICategoryService>();
            mockService.Setup(s => s.GetAllAsync()).ReturnsAsync([]);
            var controller = CreateController(mockService.Object);

            await controller.GetAllAsync();

            mockService.Verify(s => s.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_PreservesSubcategories()
        {
            var subs = new List<SubcategoryDto>
            {
                new() { Id = 10, Name = "Car", Description = "Car expenses" },
                new() { Id = 11, Name = "Bus" }
            };
            var categories = new List<CategoryDto>
            {
                new() { Id = 1, Name = "Transport", Description = "Transport costs", Subcategories = subs }
            };
            var mockService = new Mock<ICategoryService>();
            mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(categories);
            var controller = CreateController(mockService.Object);

            var result = await controller.GetAllAsync();

            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsAssignableFrom<IEnumerable<CategoryDto>>(ok.Value).ToList();
            Assert.Equal(2, returned[0].Subcategories.Count());
            Assert.Equal("Car expenses", returned[0].Subcategories.First().Description);
        }
    }
}
