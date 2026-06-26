using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Touir.ExpensesManager.Expenses.Controllers.Responses;
using Touir.ExpensesManager.Expenses.Infrastructure;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Controllers
{
    [Route("export")]
    [ApiController]
    [EnableRateLimiting("expenses_global")]
    public class ExpenseExportController : ControllerBase
    {
        private readonly IExpenseExportService _exportService;

        public ExpenseExportController(IExpenseExportService exportService)
        {
            _exportService = exportService;
        }

        /// <summary>
        /// Export all expenses for the authenticated user as a CSV file.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ExportAsync(CancellationToken cancellationToken)
        {
            var userId = JwtCookieReader.GetUserId(Request);
            if (userId is null)
                return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

            var stream = await _exportService.ExportCsvAsync(userId.Value, cancellationToken);
            var fileName = $"expenses_{DateTime.UtcNow:yyyyMMdd}.csv";
            return File(stream, "text/csv", fileName);
        }
    }
}
