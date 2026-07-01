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

        private static IFormFile MakeFormFile(
            string content = "date,amount,currency_code,category,subcategory,description,tags,families\n2025-01-01,10,EUR,,,,,,",
            string contentType = "text/csv",
            string fileName = "test.csv")
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            var file = new Mock<IFormFile>();
            file.Setup(f => f.Length).Returns(bytes.Length);
            file.Setup(f => f.FileName).Returns(fileName);
            file.Setup(f => f.ContentType).Returns(contentType);
            file.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns<Stream, CancellationToken>(async (target, _) => await target.WriteAsync(bytes));
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
        public async Task PreviewAsync_Returns400_WhenWrongExtension()
        {
            var file = MakeFormFile(fileName: "upload.exe", contentType: "text/csv");
            var result = await CreateController().PreviewAsync(file);
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var err = Assert.IsType<ErrorResponse>(bad.Value);
            Assert.Equal("INVALID_FILE_TYPE", err.Message);
        }

        [Fact]
        public async Task PreviewAsync_Returns400_WhenWrongContentType()
        {
            var file = MakeFormFile(fileName: "test.csv", contentType: "application/octet-stream");
            var result = await CreateController().PreviewAsync(file);
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var err = Assert.IsType<ErrorResponse>(bad.Value);
            Assert.Equal("INVALID_FILE_TYPE", err.Message);
        }

        [Fact]
        public async Task PreviewAsync_Returns400_WhenFileTooLarge()
        {
            var oversizedFile = new Mock<IFormFile>();
            oversizedFile.Setup(f => f.Length).Returns(2 * 1024 * 1024); // 2 MB
            oversizedFile.Setup(f => f.FileName).Returns("test.csv");
            oversizedFile.Setup(f => f.ContentType).Returns("text/csv");
            var result = await CreateController().PreviewAsync(oversizedFile.Object);
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var err = Assert.IsType<ErrorResponse>(bad.Value);
            Assert.Equal("IMPORT_FILE_TOO_LARGE", err.Message);
        }

        [Fact]
        public async Task PreviewAsync_Returns200_WithPreview()
        {
            var preview = new CsvImportPreviewDto { TotalRows = 2, ValidCount = 2, ErrorCount = 0 };
            var svc = new Mock<ICsvImportService>();
            svc.Setup(s => s.ParseAndValidateAsync(It.IsAny<Stream>(), It.IsAny<int>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(preview);

            var result = await CreateController(svc.Object).PreviewAsync(MakeFormFile());

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(preview, ok.Value);
        }

        [Fact]
        public async Task PreviewAsync_Returns400_OnException()
        {
            var svc = new Mock<ICsvImportService>();
            svc.Setup(s => s.ParseAndValidateAsync(It.IsAny<Stream>(), It.IsAny<int>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<CancellationToken>()))
               .ThrowsAsync(new Exception("parse error"));

            var result = await CreateController(svc.Object).PreviewAsync(MakeFormFile());
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task PreviewAsync_Returns400_OnTimeout()
        {
            var svc = new Mock<ICsvImportService>();
            svc.Setup(s => s.ParseAndValidateAsync(It.IsAny<Stream>(), It.IsAny<int>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<CancellationToken>()))
               .ThrowsAsync(new OperationCanceledException());

            var result = await CreateController(svc.Object).PreviewAsync(MakeFormFile());
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var err = Assert.IsType<ErrorResponse>(bad.Value);
            Assert.Equal("IMPORT_TIMEOUT", err.Message);
        }

        [Fact]
        public async Task PreviewAsync_Returns400_WithMissingHeadersCode_InsteadOfGenericServerError()
        {
            var svc = new Mock<ICsvImportService>();
            svc.Setup(s => s.ParseAndValidateAsync(It.IsAny<Stream>(), It.IsAny<int>(), It.IsAny<Dictionary<string, string>?>(), It.IsAny<CancellationToken>()))
               .ThrowsAsync(new InvalidOperationException("MISSING_HEADERS:date,amount"));

            var result = await CreateController(svc.Object).PreviewAsync(MakeFormFile());

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var err = Assert.IsType<ErrorResponse>(bad.Value);
            Assert.Equal("MISSING_HEADERS:date,amount", err.Message);
        }

        [Fact]
        public async Task PreviewAsync_Returns400_WithInvalidColumnMapping_OnMalformedMappingJson()
        {
            var result = await CreateController().PreviewAsync(MakeFormFile(), columnMapping: "{not valid json");

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var err = Assert.IsType<ErrorResponse>(bad.Value);
            Assert.Equal("INVALID_COLUMN_MAPPING", err.Message);
        }

        // ── DetectHeadersAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task DetectHeadersAsync_Returns401_WhenNoCookie()
        {
            var controller = CreateController(jwtCookie: null);
            var result = await controller.DetectHeadersAsync(MakeFormFile());
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task DetectHeadersAsync_Returns400_WhenNoFile()
        {
            var result = await CreateController().DetectHeadersAsync(null);
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var err = Assert.IsType<ErrorResponse>(bad.Value);
            Assert.Equal("IMPORT_NO_FILE", err.Message);
        }

        [Fact]
        public async Task DetectHeadersAsync_Returns400_WhenWrongExtension()
        {
            var file = MakeFormFile(fileName: "upload.exe", contentType: "text/csv");
            var result = await CreateController().DetectHeadersAsync(file);
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var err = Assert.IsType<ErrorResponse>(bad.Value);
            Assert.Equal("INVALID_FILE_TYPE", err.Message);
        }

        [Fact]
        public async Task DetectHeadersAsync_Returns200_WithDetectionResult()
        {
            var detection = new CsvHeaderDetectionDto { RawHeaders = ["sum", "cur"], HeadersMatchExactly = false };
            var svc = new Mock<ICsvImportService>();
            svc.Setup(s => s.DetectHeadersAsync(It.IsAny<Stream>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(detection);

            var result = await CreateController(svc.Object).DetectHeadersAsync(MakeFormFile());

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(detection, ok.Value);
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

        // ── ValidateRowsAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task ValidateRowsAsync_Returns401_WhenNoCookie()
        {
            var controller = CreateController(jwtCookie: null);
            var result = await controller.ValidateRowsAsync(new ValidateRowsRequest
            {
                Rows = [new RawCsvRowDto { RowNumber = 1, Date = "2025-01-01", Amount = "10", CurrencyCode = "EUR" }]
            });
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task ValidateRowsAsync_Returns200_WithPreview()
        {
            var preview = new CsvImportPreviewDto { TotalRows = 1, ValidCount = 1, ErrorCount = 0 };
            var svc = new Mock<ICsvImportService>();
            svc.Setup(s => s.ValidateRowsAsync(It.IsAny<IEnumerable<RawCsvRowDto>>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(preview);

            var result = await CreateController(svc.Object).ValidateRowsAsync(new ValidateRowsRequest
            {
                Rows = [new RawCsvRowDto { RowNumber = 1, Date = "2025-01-01", Amount = "10", CurrencyCode = "EUR" }]
            });

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(preview, ok.Value);
        }

        [Fact]
        public async Task ValidateRowsAsync_Returns400_OnException()
        {
            var svc = new Mock<ICsvImportService>();
            svc.Setup(s => s.ValidateRowsAsync(It.IsAny<IEnumerable<RawCsvRowDto>>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
               .ThrowsAsync(new Exception("error"));

            var result = await CreateController(svc.Object).ValidateRowsAsync(new ValidateRowsRequest
            {
                Rows = [new RawCsvRowDto { RowNumber = 1 }]
            });

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ValidateRowsAsync_Returns400_OnTimeout()
        {
            var svc = new Mock<ICsvImportService>();
            svc.Setup(s => s.ValidateRowsAsync(It.IsAny<IEnumerable<RawCsvRowDto>>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
               .ThrowsAsync(new OperationCanceledException());

            var result = await CreateController(svc.Object).ValidateRowsAsync(new ValidateRowsRequest
            {
                Rows = [new RawCsvRowDto { RowNumber = 1 }]
            });

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var err = Assert.IsType<ErrorResponse>(bad.Value);
            Assert.Equal("IMPORT_TIMEOUT", err.Message);
        }
    }
}
