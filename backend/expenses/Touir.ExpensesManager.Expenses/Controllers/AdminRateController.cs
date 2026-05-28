using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Requests;
using Touir.ExpensesManager.Expenses.Controllers.Responses;
using Touir.ExpensesManager.Expenses.Filters;
using Touir.ExpensesManager.Expenses.Infrastructure;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Controllers
{
    [Route("admin/rates")]
    [ApiController]
    [AppAdmin]
    [EnableRateLimiting("expenses_global")]
    public class AdminRateController : ControllerBase
    {
        private readonly ICurrencyRateService _rateService;

        public AdminRateController(ICurrencyRateService rateService)
        {
            _rateService = rateService;
        }

        /// <summary>
        /// Get exchange rate history for a currency pair.
        /// </summary>
        [HttpGet("history", Name = "GetAdminRateHistory")]
        [ProducesResponseType(typeof(PagedRatesResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetHistoryAsync([FromQuery] int? sourceCurrencyId, [FromQuery] int? destinationCurrencyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                var result = await _rateService.GetRateHistoryAsync(sourceCurrencyId, destinationCurrencyId, page, pageSize);
                return Ok(result);
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>
        /// Add a manual exchange rate for a specific date.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(RateDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddRateAsync(AddRateRequest request)
        {
            var userId = JwtCookieReader.GetUserId(Request);
            if (userId is null)
                return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

            try
            {
                var dto = await _rateService.AddManualRateAsync(request, userId.Value);
                return CreatedAtRoute("GetAdminRateHistory", new
                {
                    sourceCurrencyId = dto.SourceCurrencyId,
                    destinationCurrencyId = dto.DestinationCurrencyId
                }, dto);
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>
        /// Add multiple manual exchange rates in bulk.
        /// </summary>
        [HttpPost("bulk")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BulkAddRatesAsync(BulkAddRatesRequest request)
        {
            var userId = JwtCookieReader.GetUserId(Request);
            if (userId is null)
                return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

            try
            {
                await _rateService.BulkAddManualRatesAsync(request, userId.Value);
                return NoContent();
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>
        /// Get all pending rate conflicts.
        /// </summary>
        [HttpGet("conflicts/pending")]
        [ProducesResponseType(typeof(IEnumerable<RateConflictDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPendingConflictsAsync()
        {
            try
            {
                var conflicts = await _rateService.GetPendingConflictsAsync();
                return Ok(conflicts);
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>
        /// Resolve a pending rate conflict.
        /// </summary>
        [HttpPut("conflicts/{id:int}/resolve")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResolveConflictAsync(int id, ResolveConflictRequest request)
        {
            var userId = JwtCookieReader.GetUserId(Request);
            if (userId is null)
                return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

            try
            {
                await _rateService.ResolveConflictAsync(id, request, userId.Value);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ErrorResponse { Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>
        /// Refresh exchange rates from the provider starting from a given date.
        /// </summary>
        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RefreshRatesAsync(RefreshRatesRequest request)
        {
            var userId = JwtCookieReader.GetUserId(Request);
            if (userId is null)
                return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

            try
            {
                await _rateService.RefreshRatesFromAsync(request.From, request.To, request.SourceCurrencyId, request.DestinationCurrencyId);
                return NoContent();
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }
    }
}
