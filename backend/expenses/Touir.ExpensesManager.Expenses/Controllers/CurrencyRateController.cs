using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Requests;
using Touir.ExpensesManager.Expenses.Controllers.Responses;
using Touir.ExpensesManager.Expenses.Infrastructure;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Controllers
{
    [Route("rates")]
    [ApiController]
    [EnableRateLimiting("expenses_global")]
    public class CurrencyRateController : ControllerBase
    {
        private readonly ICurrencyRateService _rateService;

        public CurrencyRateController(ICurrencyRateService rateService)
        {
            _rateService = rateService;
        }

        /// <summary>
        /// Get exchange rate history for a currency pair.
        /// </summary>
        [HttpGet("history", Name = "GetRateHistory")]
        [ProducesResponseType(typeof(IEnumerable<RateDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetHistoryAsync([FromQuery] int sourceCurrencyId, [FromQuery] int destinationCurrencyId)
        {
            try
            {
                var rates = await _rateService.GetRateHistoryAsync(sourceCurrencyId, destinationCurrencyId);
                return Ok(rates);
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>
        /// Add a manual exchange rate for a specific date.
        /// </summary>
        [HttpPost(Name = "AddRate")]
        [ProducesResponseType(typeof(RateDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddRateAsync(AddRateRequest request)
        {
            try
            {
                var userId = JwtCookieReader.GetUserId(Request);
                if (userId is null)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

                var dto = await _rateService.AddManualRateAsync(request, userId.Value);
                return CreatedAtRoute("GetRateHistory", new
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
            try
            {
                var userId = JwtCookieReader.GetUserId(Request);
                if (userId is null)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

                await _rateService.BulkAddManualRatesAsync(request, userId.Value);
                return NoContent();
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>
        /// Set the default fallback rate for a currency pair.
        /// </summary>
        [HttpPut("default")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SetDefaultAsync(SetDefaultRateRequest request)
        {
            try
            {
                var userId = JwtCookieReader.GetUserId(Request);
                if (userId is null)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

                await _rateService.SetDefaultFallbackAsync(request, userId.Value);
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
        [HttpGet("conflicts")]
        [ProducesResponseType(typeof(IEnumerable<RateConflictDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetConflictsAsync()
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
        /// Refresh exchange rates from the provider starting from a given date.
        /// </summary>
        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RefreshRatesAsync(RefreshRatesRequest request)
        {
            try
            {
                var userId = JwtCookieReader.GetUserId(Request);
                if (userId is null)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

                await _rateService.RefreshRatesFromAsync(request.From, request.SourceCurrencyId, request.DestinationCurrencyId);
                return NoContent();
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>
        /// Resolve a pending rate conflict.
        /// </summary>
        [HttpPost("conflicts/{id:int}/resolve")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResolveConflictAsync(int id, ResolveConflictRequest request)
        {
            try
            {
                var userId = JwtCookieReader.GetUserId(Request);
                if (userId is null)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

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
    }
}
