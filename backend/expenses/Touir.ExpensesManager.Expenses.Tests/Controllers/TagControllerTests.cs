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
    public class TagControllerTests
    {
        // JWT sub=42
        private const string FakeJwt =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9" +
            ".eyJzdWIiOiI0MiIsImV4cCI6OTk5OTk5OTk5OX0" +
            ".placeholder";

        private static TagController CreateController(
            ITagService? service = null,
            string? jwtCookie = FakeJwt)
        {
            var controller = new TagController(service ?? Mock.Of<ITagService>());
            var httpContext = new DefaultHttpContext();
            if (jwtCookie is not null)
                httpContext.Request.Headers.Cookie = $"auth_token={jwtCookie}";
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            return controller;
        }

        private static TagDto MakeTagDto(int id = 1, string name = "food") => new() { Id = id, Name = name };

        private static TagListDto MakeTagListDto(IEnumerable<TagDto>? own = null, IEnumerable<TagDto>? family = null) =>
            new() { Own = own ?? [], Family = family ?? [] };

        // ── GetTagsAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetTags_Returns401_WhenNoCookie()
        {
            var result = await CreateController(jwtCookie: null).GetTagsAsync();
            var response = Assert.IsType<UnauthorizedObjectResult>(result);
            var error = Assert.IsType<ErrorResponse>(response.Value);
            Assert.Equal("UNAUTHORIZED", error.Message);
        }

        [Fact]
        public async Task GetTags_Returns200_WithTagList()
        {
            var tagList = MakeTagListDto(own: [MakeTagDto(1, "food"), MakeTagDto(2, "travel")]);
            var service = new Mock<ITagService>();
            service.Setup(s => s.GetVisibleAsync(42)).ReturnsAsync(tagList);

            var result = await CreateController(service.Object).GetTagsAsync();

            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<TagListDto>(ok.Value);
            Assert.Equal(2, returned.Own.Count());
            Assert.Empty(returned.Family);
        }

        [Fact]
        public async Task GetTags_Returns200_WithEmptyLists()
        {
            var tagList = MakeTagListDto();
            var service = new Mock<ITagService>();
            service.Setup(s => s.GetVisibleAsync(42)).ReturnsAsync(tagList);

            var result = await CreateController(service.Object).GetTagsAsync();

            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<TagListDto>(ok.Value);
            Assert.Empty(returned.Own);
            Assert.Empty(returned.Family);
        }

        [Fact]
        public async Task GetTags_Returns200_WithFamilyTags()
        {
            var tagList = MakeTagListDto(family: [MakeTagDto(5, "shared")]);
            var service = new Mock<ITagService>();
            service.Setup(s => s.GetVisibleAsync(42)).ReturnsAsync(tagList);

            var result = await CreateController(service.Object).GetTagsAsync();

            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<TagListDto>(ok.Value);
            Assert.Empty(returned.Own);
            Assert.Single(returned.Family);
        }

        [Fact]
        public async Task GetTags_CallsServiceWithCorrectUserId()
        {
            var service = new Mock<ITagService>();
            service.Setup(s => s.GetVisibleAsync(42)).ReturnsAsync(MakeTagListDto());

            await CreateController(service.Object).GetTagsAsync();

            service.Verify(s => s.GetVisibleAsync(42), Times.Once);
        }

        // ── UseTagAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task UseTag_Returns401_WhenNoCookie()
        {
            var result = await CreateController(jwtCookie: null).UseTagAsync(new CreateTagRequest { Name = "food" });
            var response = Assert.IsType<UnauthorizedObjectResult>(result);
            var error = Assert.IsType<ErrorResponse>(response.Value);
            Assert.Equal("UNAUTHORIZED", error.Message);
        }

        [Fact]
        public async Task UseTag_Returns200_WithCreatedTag()
        {
            var tag = MakeTagDto(1, "food");
            var service = new Mock<ITagService>();
            service.Setup(s => s.UseTagAsync("food", 42)).ReturnsAsync(tag);

            var result = await CreateController(service.Object).UseTagAsync(new CreateTagRequest { Name = "food" });

            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<TagDto>(ok.Value);
            Assert.Equal(1, returned.Id);
            Assert.Equal("food", returned.Name);
        }

        [Fact]
        public async Task UseTag_Returns200_WhenTagAlreadyExists()
        {
            var existing = MakeTagDto(7, "food");
            var service = new Mock<ITagService>();
            service.Setup(s => s.UseTagAsync("food", 42)).ReturnsAsync(existing);

            var result = await CreateController(service.Object).UseTagAsync(new CreateTagRequest { Name = "food" });

            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<TagDto>(ok.Value);
            Assert.Equal(7, returned.Id);
        }

        [Fact]
        public async Task UseTag_CallsServiceWithCorrectArgs()
        {
            var service = new Mock<ITagService>();
            service.Setup(s => s.UseTagAsync(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(MakeTagDto());

            await CreateController(service.Object).UseTagAsync(new CreateTagRequest { Name = "travel" });

            service.Verify(s => s.UseTagAsync("travel", 42), Times.Once);
        }

        // ── RemoveTagAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task RemoveTag_Returns401_WhenNoCookie()
        {
            var result = await CreateController(jwtCookie: null).RemoveTagAsync(1);
            var response = Assert.IsType<UnauthorizedObjectResult>(result);
            var error = Assert.IsType<ErrorResponse>(response.Value);
            Assert.Equal("UNAUTHORIZED", error.Message);
        }

        [Fact]
        public async Task RemoveTag_Returns204_WhenRemoved()
        {
            var service = new Mock<ITagService>();
            service.Setup(s => s.RemoveTagAsync(1, 42)).ReturnsAsync(true);

            var result = await CreateController(service.Object).RemoveTagAsync(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task RemoveTag_Returns404_WhenNotFound()
        {
            var service = new Mock<ITagService>();
            service.Setup(s => s.RemoveTagAsync(99, 42)).ReturnsAsync(false);

            var result = await CreateController(service.Object).RemoveTagAsync(99);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            var error = Assert.IsType<ErrorResponse>(notFound.Value);
            Assert.Equal("TAG_NOT_FOUND", error.Message);
        }

        [Fact]
        public async Task RemoveTag_CallsServiceWithCorrectArgs()
        {
            var service = new Mock<ITagService>();
            service.Setup(s => s.RemoveTagAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(true);

            await CreateController(service.Object).RemoveTagAsync(5);

            service.Verify(s => s.RemoveTagAsync(5, 42), Times.Once);
        }
    }
}
