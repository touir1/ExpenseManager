using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Repositories.Contracts;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Services
{
    public class ReceiptService : IReceiptService
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly IExpenseService _expenseService;
        private readonly IReceiptStorageService _receiptStorageService;

        public ReceiptService(
            IExpenseRepository expenseRepository,
            IExpenseService expenseService,
            IReceiptStorageService receiptStorageService)
        {
            _expenseRepository = expenseRepository;
            _expenseService = expenseService;
            _receiptStorageService = receiptStorageService;
        }

        public async Task<ExpenseDto?> UploadAsync(long expenseId, int userId, Stream content, string contentType, string extension, CancellationToken cancellationToken = default)
        {
            var existing = await _expenseRepository.GetByIdAsync(expenseId, userId);
            if (existing is null)
                return null;

            var previousStorageKey = existing.ReceiptStorageKey;

            var newStorageKey = await _receiptStorageService.UploadAsync(content, contentType, expenseId, extension, cancellationToken);

            existing.ReceiptStorageKey = newStorageKey;
            await _expenseRepository.UpdateAsync(existing);

            if (previousStorageKey is not null)
                await _receiptStorageService.DeleteAsync(previousStorageKey, cancellationToken);

            return await _expenseService.GetByIdAsync(expenseId, userId);
        }

        public async Task<ReceiptGetResult> GetAsync(long expenseId, int userId, CancellationToken cancellationToken = default)
        {
            var existing = await _expenseRepository.GetByIdAsync(expenseId, userId);
            if (existing is null)
                return new ReceiptGetResult { ExpenseFound = false, HasReceipt = false };

            if (existing.ReceiptStorageKey is null)
                return new ReceiptGetResult { ExpenseFound = true, HasReceipt = false };

            var (stream, contentType) = await _receiptStorageService.GetStreamAsync(existing.ReceiptStorageKey, cancellationToken);
            return new ReceiptGetResult { ExpenseFound = true, HasReceipt = true, Stream = stream, ContentType = contentType };
        }

        public async Task<ReceiptDeleteResult> DeleteAsync(long expenseId, int userId, CancellationToken cancellationToken = default)
        {
            var existing = await _expenseRepository.GetByIdAsync(expenseId, userId);
            if (existing is null)
                return new ReceiptDeleteResult { ExpenseFound = false, HadReceipt = false };

            if (existing.ReceiptStorageKey is null)
                return new ReceiptDeleteResult { ExpenseFound = true, HadReceipt = false };

            var storageKey = existing.ReceiptStorageKey;
            existing.ReceiptStorageKey = null;
            await _expenseRepository.UpdateAsync(existing);
            await _receiptStorageService.DeleteAsync(storageKey, cancellationToken);

            return new ReceiptDeleteResult { ExpenseFound = true, HadReceipt = true };
        }
    }
}
