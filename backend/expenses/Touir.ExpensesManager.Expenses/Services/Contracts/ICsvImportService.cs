using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Requests;

namespace Touir.ExpensesManager.Expenses.Services.Contracts
{
    public interface ICsvImportService
    {
        Task<CsvImportPreviewDto> ParseAndValidateAsync(Stream csvStream, int userId);
        Task<CsvImportPreviewDto> ValidateRowsAsync(IEnumerable<RawCsvRowDto> rows, int userId);
        Task<CsvImportResultDto> ConfirmImportAsync(IEnumerable<CsvImportConfirmRowDto> rows, int userId);
    }
}
