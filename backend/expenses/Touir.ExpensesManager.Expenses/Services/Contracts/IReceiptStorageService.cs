namespace Touir.ExpensesManager.Expenses.Services.Contracts
{
    public interface IReceiptStorageService
    {
        Task<string> UploadAsync(Stream content, string contentType, long expenseId, string extension, CancellationToken cancellationToken = default);
        Task<(Stream Stream, string ContentType)> GetStreamAsync(string storageKey, CancellationToken cancellationToken = default);
        Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
    }
}
