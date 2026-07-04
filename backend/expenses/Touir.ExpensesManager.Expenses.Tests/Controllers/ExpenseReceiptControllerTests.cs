using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Touir.ExpensesManager.Expenses.Controllers;
using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Responses;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Tests.Controllers
{
    public class ExpenseReceiptControllerTests
    {
        // JWT: sub=42, exp far future
        private const string FakeJwt =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9" +
            ".eyJzdWIiOiI0MiIsImV4cCI6OTk5OTk5OTk5OX0" +
            ".placeholder";

        private static ExpenseReceiptController CreateController(
            IReceiptService? service = null,
            string? jwtCookie = FakeJwt)
        {
            var controller = new ExpenseReceiptController(service ?? Mock.Of<IReceiptService>());
            var httpContext = new DefaultHttpContext();
            if (jwtCookie is not null)
                httpContext.Request.Headers.Cookie = $"auth_token={jwtCookie}";
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            return controller;
        }

        private static IFormFile MakeFormFile(
            byte[]? bytes = null,
            string contentType = "image/jpeg",
            string fileName = "receipt.jpg")
        {
            bytes ??= [1, 2, 3, 4];
            var file = new Mock<IFormFile>();
            file.Setup(f => f.Length).Returns(bytes.Length);
            file.Setup(f => f.FileName).Returns(fileName);
            file.Setup(f => f.ContentType).Returns(contentType);
            file.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns<Stream, CancellationToken>(async (target, _) => await target.WriteAsync(bytes));
            return file.Object;
        }

        // ── UploadAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task UploadAsync_Returns401_WhenNoCookie()
        {
            var result = await CreateController(jwtCookie: null).UploadAsync(1, MakeFormFile(), CancellationToken.None);
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task UploadAsync_Returns400_WhenNoFile()
        {
            var result = await CreateController().UploadAsync(1, null, CancellationToken.None);
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var err = Assert.IsType<ErrorResponse>(bad.Value);
            Assert.Equal("RECEIPT_NO_FILE", err.Message);
        }

        [Fact]
        public async Task UploadAsync_Returns400_WhenFileTooLarge()
        {
            var file = MakeFormFile(bytes: new byte[6 * 1024 * 1024]);
            var result = await CreateController().UploadAsync(1, file, CancellationToken.None);
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var err = Assert.IsType<ErrorResponse>(bad.Value);
            Assert.Equal("RECEIPT_FILE_TOO_LARGE", err.Message);
        }

        [Fact]
        public async Task UploadAsync_Returns400_WhenWrongExtension()
        {
            var file = MakeFormFile(fileName: "receipt.pdf");
            var result = await CreateController().UploadAsync(1, file, CancellationToken.None);
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var err = Assert.IsType<ErrorResponse>(bad.Value);
            Assert.Equal("RECEIPT_INVALID_FILE_TYPE", err.Message);
        }

        [Fact]
        public async Task UploadAsync_Returns400_WhenWrongContentType()
        {
            var file = MakeFormFile(contentType: "application/pdf");
            var result = await CreateController().UploadAsync(1, file, CancellationToken.None);
            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var err = Assert.IsType<ErrorResponse>(bad.Value);
            Assert.Equal("RECEIPT_INVALID_FILE_TYPE", err.Message);
        }

        [Fact]
        public async Task UploadAsync_Returns404_WhenExpenseNotOwned()
        {
            var service = new Mock<IReceiptService>();
            service.Setup(s => s.UploadAsync(1, 42, It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync((ExpenseDto?)null);

            var result = await CreateController(service.Object).UploadAsync(1, MakeFormFile(), CancellationToken.None);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            var err = Assert.IsType<ErrorResponse>(notFound.Value);
            Assert.Equal("EXPENSE_NOT_FOUND", err.Message);
        }

        [Fact]
        public async Task UploadAsync_Returns200_OnSuccess()
        {
            var dto = new ExpenseDto { Id = 1, HasReceipt = true };
            var service = new Mock<IReceiptService>();
            service.Setup(s => s.UploadAsync(1, 42, It.IsAny<Stream>(), "image/jpeg", ".jpg", It.IsAny<CancellationToken>()))
                   .ReturnsAsync(dto);

            var result = await CreateController(service.Object).UploadAsync(1, MakeFormFile(), CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<ExpenseDto>(ok.Value);
            Assert.True(returned.HasReceipt);
        }

        [Fact]
        public async Task UploadAsync_Returns400_OnServiceException()
        {
            var service = new Mock<IReceiptService>();
            service.Setup(s => s.UploadAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new Exception("boom"));

            var result = await CreateController(service.Object).UploadAsync(1, MakeFormFile(), CancellationToken.None);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var err = Assert.IsType<ErrorResponse>(bad.Value);
            Assert.Equal("SERVER_ERROR", err.Message);
        }

        // ── GetAsync ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAsync_Returns401_WhenNoCookie()
        {
            var result = await CreateController(jwtCookie: null).GetAsync(1, false, CancellationToken.None);
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task GetAsync_Returns404_WhenExpenseNotFound()
        {
            var service = new Mock<IReceiptService>();
            service.Setup(s => s.GetAsync(1, 42, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new ReceiptGetResult { ExpenseFound = false, HasReceipt = false });

            var result = await CreateController(service.Object).GetAsync(1, false, CancellationToken.None);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            var err = Assert.IsType<ErrorResponse>(notFound.Value);
            Assert.Equal("EXPENSE_NOT_FOUND", err.Message);
        }

        [Fact]
        public async Task GetAsync_Returns404_WhenNoReceipt()
        {
            var service = new Mock<IReceiptService>();
            service.Setup(s => s.GetAsync(1, 42, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new ReceiptGetResult { ExpenseFound = true, HasReceipt = false });

            var result = await CreateController(service.Object).GetAsync(1, false, CancellationToken.None);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            var err = Assert.IsType<ErrorResponse>(notFound.Value);
            Assert.Equal("RECEIPT_NOT_FOUND", err.Message);
        }

        [Fact]
        public async Task GetAsync_ReturnsFileResult_ForInlineView()
        {
            var service = new Mock<IReceiptService>();
            service.Setup(s => s.GetAsync(1, 42, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new ReceiptGetResult
                   {
                       ExpenseFound = true,
                       HasReceipt = true,
                       Stream = new MemoryStream([1, 2, 3]),
                       ContentType = "image/jpeg"
                   });

            var result = await CreateController(service.Object).GetAsync(1, false, CancellationToken.None);

            var file = Assert.IsType<FileStreamResult>(result);
            Assert.Equal("image/jpeg", file.ContentType);
            Assert.True(string.IsNullOrEmpty(file.FileDownloadName));
        }

        [Fact]
        public async Task GetAsync_ReturnsAttachment_WhenDownloadTrue()
        {
            var service = new Mock<IReceiptService>();
            service.Setup(s => s.GetAsync(1, 42, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new ReceiptGetResult
                   {
                       ExpenseFound = true,
                       HasReceipt = true,
                       Stream = new MemoryStream([1, 2, 3]),
                       ContentType = "image/png"
                   });

            var result = await CreateController(service.Object).GetAsync(1, true, CancellationToken.None);

            var file = Assert.IsType<FileStreamResult>(result);
            Assert.Equal("image/png", file.ContentType);
            Assert.Equal("receipt-1.png", file.FileDownloadName);
        }

        [Fact]
        public async Task GetAsync_Returns400_OnServiceException()
        {
            var service = new Mock<IReceiptService>();
            service.Setup(s => s.GetAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new Exception("boom"));

            var result = await CreateController(service.Object).GetAsync(1, false, CancellationToken.None);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // ── DeleteAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteAsync_Returns401_WhenNoCookie()
        {
            var result = await CreateController(jwtCookie: null).DeleteAsync(1, CancellationToken.None);
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task DeleteAsync_Returns404_WhenExpenseNotFound()
        {
            var service = new Mock<IReceiptService>();
            service.Setup(s => s.DeleteAsync(1, 42, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new ReceiptDeleteResult { ExpenseFound = false, HadReceipt = false });

            var result = await CreateController(service.Object).DeleteAsync(1, CancellationToken.None);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            var err = Assert.IsType<ErrorResponse>(notFound.Value);
            Assert.Equal("EXPENSE_NOT_FOUND", err.Message);
        }

        [Fact]
        public async Task DeleteAsync_Returns404_WhenNoReceipt()
        {
            var service = new Mock<IReceiptService>();
            service.Setup(s => s.DeleteAsync(1, 42, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new ReceiptDeleteResult { ExpenseFound = true, HadReceipt = false });

            var result = await CreateController(service.Object).DeleteAsync(1, CancellationToken.None);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            var err = Assert.IsType<ErrorResponse>(notFound.Value);
            Assert.Equal("RECEIPT_NOT_FOUND", err.Message);
        }

        [Fact]
        public async Task DeleteAsync_Returns204_OnSuccess()
        {
            var service = new Mock<IReceiptService>();
            service.Setup(s => s.DeleteAsync(1, 42, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new ReceiptDeleteResult { ExpenseFound = true, HadReceipt = true });

            var result = await CreateController(service.Object).DeleteAsync(1, CancellationToken.None);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteAsync_Returns400_OnServiceException()
        {
            var service = new Mock<IReceiptService>();
            service.Setup(s => s.DeleteAsync(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new Exception("boom"));

            var result = await CreateController(service.Object).DeleteAsync(1, CancellationToken.None);

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
