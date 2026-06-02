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
    public class ExpenseImportControllerTests
    {
        // JWT: sub=42, exp far future
        private const string FakeJwt =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9" +
            ".eyJzdWIiOiI0MiIsImV4cCI6OTk5OTk5OTk5OX0" +
            ".placeholder";

        private static ExpenseImportController CreateController(
            ICsvImportService? service = null,
            string? jwtCookie = FakeJwt)
        {
            var controller = new ExpenseImportController(service ?? Mock.Of<ICsvImportService>());
            var httpContext = new DefaultHttpContext();
            if (jwtCookie is not null)
                httpContext.Request.Headers.Cookie = $"auth_token={jwtCookie}";
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            return controller;
        }

        private static IFormFile MakeFormFile(string content = "date,amount,currency_code,category,subcategory,description,tags,families\n2025-01-01,10,EUR,,,,,,")
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);
            var file = new Mock<IFormFile>();
            file.Setup(f => f.Length).Returns(bytes.Length);
            file.Setup(f => f.OpenReadStream()).Returns(stream);
            return file.Object;
        }

        // ── PreviewAsync ─────────────────────────────────────────────────────────

        [Fact]
        public async Task PreviewAsync_Returns401_WhenNoCookie()
        {
            var controller = CreateController(jwtCookie: null);
            var result = await controller.PreviewAsync(MakeFormFile());
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task PreviewAsync_Returns400_WhenNoFile()
        {
            var result = await CreateController().PreviewAsync(null);
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var err = Assert.IsType<ErrorResponse>(bad.Value);
            Assert.Equal("IMPORT_NO_FILE", err.Message);
        }

        [Fact]
        public async Task PreviewAsync_Returns400_WhenEmptyFile()
        {
            var emptyFile = new Mock<IFormFile>();
            emptyFile.Setup(f => f.Length).Returns(0);
            var result = await CreateController().PreviewAsync(emptyFile.Object);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task PreviewAsync_Returns200_WithPreview()
        {
            var preview = new CsvImportPreviewDto { TotalRows = 2, ValidCount = 2, ErrorCount = 0 };
            var svc = new Mock<ICsvImportService>();
            svc.Setup(s => s.ParseAndValidateAsync(It.IsAny<Stream>(), It.IsAny<int>()))
               .ReturnsAsync(preview);

            var result = await CreateController(svc.Object).PreviewAsync(MakeFormFile());

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(preview, ok.Value);
        }

        [Fact]
        public async Task PreviewAsync_Returns400_OnException()
        {
            var svc = new Mock<ICsvImportService>();
            svc.Setup(s => s.ParseAndValidateAsync(It.IsAny<Stream>(), It.IsAny<int>()))
               .ThrowsAsync(new Exception("parse error"));

            var result = await CreateController(svc.Object).PreviewAsync(MakeFormFile());
            Assert.IsType<BadRequestObjectResult>(result);
        }

        // ── ConfirmAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task ConfirmAsync_Returns401_WhenNoCookie()
        {
            var controller = CreateController(jwtCookie: null);
            var result = await controller.ConfirmAsync(new CsvImportConfirmRequest
            {
                Rows = [new CsvImportConfirmRowDto { Amount = 10, CurrencyId = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)) }]
            });
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task ConfirmAsync_Returns200_WithResult()
        {
            var importResult = new CsvImportResultDto { Imported = 3, Skipped = 0 };
            var svc = new Mock<ICsvImportService>();
            svc.Setup(s => s.ConfirmImportAsync(It.IsAny<IEnumerable<CsvImportConfirmRowDto>>(), It.IsAny<int>()))
               .ReturnsAsync(importResult);

            var result = await CreateController(svc.Object).ConfirmAsync(new CsvImportConfirmRequest
            {
                Rows = [new CsvImportConfirmRowDto { Amount = 10, CurrencyId = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)) }]
            });

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(importResult, ok.Value);
        }

        [Fact]
        public async Task ConfirmAsync_Returns400_OnException()
        {
            var svc = new Mock<ICsvImportService>();
            svc.Setup(s => s.ConfirmImportAsync(It.IsAny<IEnumerable<CsvImportConfirmRowDto>>(), It.IsAny<int>()))
               .ThrowsAsync(new Exception("db error"));

            var result = await CreateController(svc.Object).ConfirmAsync(new CsvImportConfirmRequest
            {
                Rows = [new CsvImportConfirmRowDto { Amount = 10, CurrencyId = 1, Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)) }]
            });

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // ── TemplateAsync ─────────────────────────────────────────────────────────

        [Fact]
        public void TemplateAsync_Returns401_WhenNoCookie()
        {
            var controller = CreateController(jwtCookie: null);
            var result = controller.TemplateAsync();
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public void TemplateAsync_ReturnsCsvFile()
        {
            var result = CreateController().TemplateAsync();
            var file = Assert.IsType<FileContentResult>(result);
            Assert.Equal("text/csv", file.ContentType);
            Assert.Equal("expenses-import-template.csv", file.FileDownloadName);
        }

        [Fact]
        public void TemplateAsync_ContainsHeaderRow()
        {
            var result = CreateController().TemplateAsync();
            var file = Assert.IsType<FileContentResult>(result);
            var content = System.Text.Encoding.UTF8.GetString(file.FileContents);
            Assert.Contains("date,amount,currency_code,category,subcategory,description,tags,families", content);
        }
    }
}
