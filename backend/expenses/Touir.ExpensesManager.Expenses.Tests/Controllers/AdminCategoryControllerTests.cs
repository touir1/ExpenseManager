using Moq;
using Touir.ExpensesManager.Expenses.Controllers;
using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Requests;
using Touir.ExpensesManager.Expenses.Controllers.Responses;
using Touir.ExpensesManager.Expenses.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Touir.ExpensesManager.Expenses.Tests.Controllers
{
    public class AdminCategoryControllerTests
    {
        private static AdminCategoryController CreateController(IAdminCategoryService? svc = null)
            => new(svc ?? Mock.Of<IAdminCategoryService>());

        private static AdminCategoryDto MakeDto(int id = 1, string name = "Food")
            => new() { Id = id, Name = name, Subcategories = [] };

        [Fact]
        public async Task GetAllAsync_ReturnsOk_WithCategories()
        {
            var svc = new Mock<IAdminCategoryService>();
            svc.Setup(s => s.GetAllAsync()).ReturnsAsync([MakeDto(), MakeDto(2, "Travel")]);

            var result = await CreateController(svc.Object).GetAllAsync();

            var ok = Assert.IsType<OkObjectResult>(result);
            var list = Assert.IsAssignableFrom<IEnumerable<AdminCategoryDto>>(ok.Value);
            Assert.Equal(2, list.Count());
        }

        [Fact]
        public async Task GetAllAsync_ReturnsBadRequest_WhenServiceThrows()
        {
            var svc = new Mock<IAdminCategoryService>();
            svc.Setup(s => s.GetAllAsync()).ThrowsAsync(new Exception("db"));

            var result = await CreateController(svc.Object).GetAllAsync();

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task AddCategoryAsync_ReturnsCreated_WhenSuccessful()
        {
            var svc = new Mock<IAdminCategoryService>();
            svc.Setup(s => s.AddCategoryAsync("Food", null)).ReturnsAsync(MakeDto());

            var result = await CreateController(svc.Object).AddCategoryAsync(new AdminCategoryRequest { Name = "Food" });

            Assert.IsType<CreatedAtRouteResult>(result);
        }

        [Fact]
        public async Task UpdateCategoryAsync_ReturnsOk_WhenFound()
        {
            var svc = new Mock<IAdminCategoryService>();
            svc.Setup(s => s.UpdateCategoryAsync(1, "NewName", null)).ReturnsAsync(MakeDto(1, "NewName"));

            var result = await CreateController(svc.Object)
                .UpdateCategoryAsync(1, new AdminCategoryRequest { Name = "NewName" });

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateCategoryAsync_ReturnsNotFound_WhenKeyNotFound()
        {
            var svc = new Mock<IAdminCategoryService>();
            svc.Setup(s => s.UpdateCategoryAsync(99, It.IsAny<string>(), It.IsAny<string?>()))
               .ThrowsAsync(new KeyNotFoundException("CATEGORY_NOT_FOUND"));

            var result = await CreateController(svc.Object)
                .UpdateCategoryAsync(99, new AdminCategoryRequest { Name = "X" });

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task ArchiveCategoryAsync_ReturnsNoContent_WhenSuccessful()
        {
            var svc = new Mock<IAdminCategoryService>();
            svc.Setup(s => s.ArchiveCategoryAsync(1)).Returns(Task.CompletedTask);

            var result = await CreateController(svc.Object).ArchiveCategoryAsync(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task ArchiveCategoryAsync_ReturnsConflict_WhenHasActiveChildren()
        {
            var svc = new Mock<IAdminCategoryService>();
            svc.Setup(s => s.ArchiveCategoryAsync(1))
               .ThrowsAsync(new InvalidOperationException("CATEGORY_HAS_ACTIVE_SUBCATEGORIES"));

            var result = await CreateController(svc.Object).ArchiveCategoryAsync(1);

            Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        public async Task AddSubcategoryAsync_ReturnsCreated_WhenSuccessful()
        {
            var sub = new AdminSubcategoryDto { Id = 5, Name = "Organic" };
            var svc = new Mock<IAdminCategoryService>();
            svc.Setup(s => s.AddSubcategoryAsync(1, "Organic", null)).ReturnsAsync(sub);

            var result = await CreateController(svc.Object)
                .AddSubcategoryAsync(1, new AdminCategoryRequest { Name = "Organic" });

            Assert.IsType<CreatedAtRouteResult>(result);
        }

        [Fact]
        public async Task AddSubcategoryAsync_ReturnsConflict_WhenParentArchived()
        {
            var svc = new Mock<IAdminCategoryService>();
            svc.Setup(s => s.AddSubcategoryAsync(1, It.IsAny<string>(), It.IsAny<string?>()))
               .ThrowsAsync(new InvalidOperationException("CATEGORY_ARCHIVED"));

            var result = await CreateController(svc.Object)
                .AddSubcategoryAsync(1, new AdminCategoryRequest { Name = "Sub" });

            Assert.IsType<ConflictObjectResult>(result);
        }
    }
}
