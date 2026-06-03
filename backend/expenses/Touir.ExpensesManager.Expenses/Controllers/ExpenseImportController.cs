using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Requests;
using Touir.ExpensesManager.Expenses.Controllers.Responses;
using Touir.ExpensesManager.Expenses.Infrastructure;
using Touir.ExpensesManager.Expenses.Services.Contracts;
using System.Text;

namespace Touir.ExpensesManager.Expenses.Controllers
{
    [Route("import")]
    [ApiController]
    [EnableRateLimiting("expenses_global")]
    public class ExpenseImportController : ControllerBase
    {
        private const long MaxFileSizeBytes = 1 * 1024 * 1024; // 1 MB
        private static readonly TimeSpan ParseTimeout = TimeSpan.FromSeconds(30);

        private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "text/csv",
            "application/csv",
            "application/vnd.ms-excel",
            "text/plain",
        };

        private readonly ICsvImportService _csvImportService;

        public ExpenseImportController(ICsvImportService csvImportService)
        {
            _csvImportService = csvImportService;
        }

        /// <summary>
        /// Parse and validate a CSV file, returning a per-row preview with errors.
        /// </summary>
        [HttpPost("preview")]
        [ProducesResponseType(typeof(CsvImportPreviewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> PreviewAsync(IFormFile? file)
        {
            var userId = JwtCookieReader.GetUserId(Request);
            if (userId is null)
                return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

            if (file is null || file.Length == 0)
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ImportNoFile });

            if (file.Length > MaxFileSizeBytes)
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ImportFileTooLarge });

            var extension = Path.GetExtension(file.FileName);
            if (!extension.Equals(".csv", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new ErrorResponse { Message = ControllerErrors.InvalidFileType });

            if (!AllowedContentTypes.Contains(file.ContentType))
                return BadRequest(new ErrorResponse { Message = ControllerErrors.InvalidFileType });

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            ms.Position = 0;

            using var cts = new CancellationTokenSource(ParseTimeout);
            try
            {
                var preview = await _csvImportService.ParseAndValidateAsync(ms, userId.Value, cts.Token);
                return Ok(preview);
            }
            catch (OperationCanceledException)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ImportTimeout });
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>
        /// Confirm and bulk-insert valid rows from a prior preview.
        /// </summary>
        [HttpPost("confirm")]
        [ProducesResponseType(typeof(CsvImportResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ConfirmAsync(CsvImportConfirmRequest request)
        {
            var userId = JwtCookieReader.GetUserId(Request);
            if (userId is null)
                return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

            try
            {
                var result = await _csvImportService.ConfirmImportAsync(request.Rows, userId.Value);
                return Ok(result);
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>
        /// Re-validate edited rows without re-uploading the file.
        /// </summary>
        [HttpPost("validate-rows")]
        [ProducesResponseType(typeof(CsvImportPreviewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ValidateRowsAsync(ValidateRowsRequest request)
        {
            var userId = JwtCookieReader.GetUserId(Request);
            if (userId is null)
                return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

            using var cts = new CancellationTokenSource(ParseTimeout);
            try
            {
                var preview = await _csvImportService.ValidateRowsAsync(request.Rows, userId.Value, cts.Token);
                return Ok(preview);
            }
            catch (OperationCanceledException)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ImportTimeout });
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>
        /// Download the CSV import template with headers and example rows.
        /// </summary>
        [HttpGet("template")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public IActionResult TemplateAsync()
        {
            var userId = JwtCookieReader.GetUserId(Request);
            if (userId is null)
                return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

            var csv = new StringBuilder();
            csv.AppendLine("date,amount,currency_code,category,subcategory,description,tags,families");
            csv.AppendLine("2025-01-15,45.50,EUR,Food,Restaurant,Lunch with client,work;client,");
            csv.AppendLine("2025-01-20,12.00,EUR,Transport,,Bus ticket,,");

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", "expenses-import-template.csv");
        }
    }
}
