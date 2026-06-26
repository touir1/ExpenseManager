namespace Touir.ExpensesManager.Expenses.Services.Contracts
{
    public interface IExpenseExportService
    {
        Task<Stream> ExportCsvAsync(int userId, CancellationToken cancellationToken = default);
    }
}
