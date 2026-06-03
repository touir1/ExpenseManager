using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Requests;

namespace Touir.ExpensesManager.Expenses.Services.Contracts
{
    public interface ICsvImportService
    {
        Task<CsvImportPreviewDto> ParseAndValidateAsync(Stream csvStream, int userId, CancellationToken cancellationToken = default);
        Task<CsvImportPreviewDto> ValidateRowsAsync(IEnumerable<RawCsvRowDto> rows, int userId, CancellationToken cancellationToken = default);
        Task<CsvImportResultDto> ConfirmImportAsync(IEnumerable<CsvImportConfirmRowDto> rows, int userId);
    }
}
