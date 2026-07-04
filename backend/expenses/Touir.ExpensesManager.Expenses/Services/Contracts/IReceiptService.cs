using Touir.ExpensesManager.Expenses.Controllers.DTO;

namespace Touir.ExpensesManager.Expenses.Services.Contracts
{
    public interface IReceiptService
    {
        Task<ExpenseDto?> UploadAsync(long expenseId, int userId, Stream content, string contentType, string extension, CancellationToken cancellationToken = default);
        Task<ReceiptGetResult> GetAsync(long expenseId, int userId, CancellationToken cancellationToken = default);
        Task<ReceiptDeleteResult> DeleteAsync(long expenseId, int userId, CancellationToken cancellationToken = default);
    }

    public class ReceiptGetResult
    {
        public bool ExpenseFound { get; init; }
        public bool HasReceipt { get; init; }
        public Stream? Stream { get; init; }
        public string? ContentType { get; init; }
    }

    public class ReceiptDeleteResult
    {
        public bool ExpenseFound { get; init; }
        public bool HadReceipt { get; init; }
    }
}
