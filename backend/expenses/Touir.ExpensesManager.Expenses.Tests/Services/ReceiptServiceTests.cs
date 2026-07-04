using Moq;
using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Models;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;
using Touir.ExpensesManager.Expenses.Services;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Tests.Services
{
    public class ReceiptServiceTests
    {
        private static ReceiptService CreateService(
            IExpenseRepository? expenseRepo = null,
            IExpenseService? expenseService = null,
            IReceiptStorageService? storageService = null)
        {
            return new ReceiptService(
                expenseRepo ?? Mock.Of<IExpenseRepository>(),
                expenseService ?? Mock.Of<IExpenseService>(),
                storageService ?? Mock.Of<IReceiptStorageService>());
        }

        private static Expense MakeExpense(long id = 1, int userId = 42, string? receiptStorageKey = null) => new()
        {
            Id = id,
            UserId = userId,
            Amount = 100m,
            CurrencyId = 1,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            CreatedAt = DateTime.UtcNow,
            CreatedById = userId,
            CreatedFromId = 1,
            ReceiptStorageKey = receiptStorageKey
        };

        // ── UploadAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task UploadAsync_ReturnsNull_WhenExpenseNotFound()
        {
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetByIdAsync(1, 42)).ReturnsAsync((Expense?)null);

            var result = await CreateService(repo.Object).UploadAsync(1, 42, new MemoryStream(), "image/jpeg", ".jpg");

            Assert.Null(result);
        }

        [Fact]
        public async Task UploadAsync_UploadsAndUpdatesExpense_WhenNoExistingReceipt()
        {
            var expense = MakeExpense();
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetByIdAsync(1, 42)).ReturnsAsync(expense);

            var storage = new Mock<IReceiptStorageService>();
            storage.Setup(s => s.UploadAsync(It.IsAny<Stream>(), "image/jpeg", 1, ".jpg", It.IsAny<CancellationToken>()))
                   .ReturnsAsync("receipts/1/new-key.jpg");

            var expenseService = new Mock<IExpenseService>();
            var expectedDto = new ExpenseDto { Id = 1, HasReceipt = true };
            expenseService.Setup(s => s.GetByIdAsync(1, 42, null)).ReturnsAsync(expectedDto);

            var result = await CreateService(repo.Object, expenseService.Object, storage.Object)
                .UploadAsync(1, 42, new MemoryStream(), "image/jpeg", ".jpg");

            Assert.Same(expectedDto, result);
            repo.Verify(r => r.UpdateAsync(It.Is<Expense>(e => e.ReceiptStorageKey == "receipts/1/new-key.jpg")), Times.Once);
            storage.Verify(s => s.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UploadAsync_DeletesOldReceipt_WhenReplacingExisting()
        {
            var expense = MakeExpense(receiptStorageKey: "receipts/1/old-key.jpg");
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetByIdAsync(1, 42)).ReturnsAsync(expense);

            var storage = new Mock<IReceiptStorageService>();
            storage.Setup(s => s.UploadAsync(It.IsAny<Stream>(), "image/png", 1, ".png", It.IsAny<CancellationToken>()))
                   .ReturnsAsync("receipts/1/new-key.png");

            var expenseService = new Mock<IExpenseService>();
            expenseService.Setup(s => s.GetByIdAsync(1, 42, null)).ReturnsAsync(new ExpenseDto { Id = 1, HasReceipt = true });

            await CreateService(repo.Object, expenseService.Object, storage.Object)
                .UploadAsync(1, 42, new MemoryStream(), "image/png", ".png");

            storage.Verify(s => s.DeleteAsync("receipts/1/old-key.jpg", It.IsAny<CancellationToken>()), Times.Once);
        }

        // ── GetAsync ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAsync_ReturnsExpenseNotFound_WhenExpenseMissing()
        {
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetByIdAsync(1, 42)).ReturnsAsync((Expense?)null);

            var result = await CreateService(repo.Object).GetAsync(1, 42);

            Assert.False(result.ExpenseFound);
            Assert.False(result.HasReceipt);
        }

        [Fact]
        public async Task GetAsync_ReturnsNoReceipt_WhenExpenseHasNoReceipt()
        {
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetByIdAsync(1, 42)).ReturnsAsync(MakeExpense());

            var result = await CreateService(repo.Object).GetAsync(1, 42);

            Assert.True(result.ExpenseFound);
            Assert.False(result.HasReceipt);
        }

        [Fact]
        public async Task GetAsync_ReturnsStream_WhenReceiptExists()
        {
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetByIdAsync(1, 42)).ReturnsAsync(MakeExpense(receiptStorageKey: "receipts/1/key.jpg"));

            var stream = new MemoryStream();
            var storage = new Mock<IReceiptStorageService>();
            storage.Setup(s => s.GetStreamAsync("receipts/1/key.jpg", It.IsAny<CancellationToken>()))
                   .ReturnsAsync((stream, "image/jpeg"));

            var result = await CreateService(repo.Object, storageService: storage.Object).GetAsync(1, 42);

            Assert.True(result.ExpenseFound);
            Assert.True(result.HasReceipt);
            Assert.Same(stream, result.Stream);
            Assert.Equal("image/jpeg", result.ContentType);
        }

        // ── DeleteAsync ──────────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteAsync_ReturnsExpenseNotFound_WhenExpenseMissing()
        {
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetByIdAsync(1, 42)).ReturnsAsync((Expense?)null);

            var result = await CreateService(repo.Object).DeleteAsync(1, 42);

            Assert.False(result.ExpenseFound);
            Assert.False(result.HadReceipt);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsNoReceipt_WhenExpenseHasNoReceipt()
        {
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetByIdAsync(1, 42)).ReturnsAsync(MakeExpense());

            var result = await CreateService(repo.Object).DeleteAsync(1, 42);

            Assert.True(result.ExpenseFound);
            Assert.False(result.HadReceipt);
        }

        [Fact]
        public async Task DeleteAsync_ClearsKeyAndDeletesStorage_WhenReceiptExists()
        {
            var expense = MakeExpense(receiptStorageKey: "receipts/1/key.jpg");
            var repo = new Mock<IExpenseRepository>();
            repo.Setup(r => r.GetByIdAsync(1, 42)).ReturnsAsync(expense);

            var storage = new Mock<IReceiptStorageService>();

            var result = await CreateService(repo.Object, storageService: storage.Object).DeleteAsync(1, 42);

            Assert.True(result.ExpenseFound);
            Assert.True(result.HadReceipt);
            repo.Verify(r => r.UpdateAsync(It.Is<Expense>(e => e.ReceiptStorageKey == null)), Times.Once);
            storage.Verify(s => s.DeleteAsync("receipts/1/key.jpg", It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
