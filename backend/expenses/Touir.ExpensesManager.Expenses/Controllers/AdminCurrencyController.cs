using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Requests;
using Touir.ExpensesManager.Expenses.Controllers.Responses;
using Touir.ExpensesManager.Expenses.Filters;
using Touir.ExpensesManager.Expenses.Infrastructure;
using Touir.ExpensesManager.Expenses.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Touir.ExpensesManager.Expenses.Controllers
{
    [Route("admin/currencies")]
    [ApiController]
    [AppAdmin]
    [EnableRateLimiting("expenses_global")]
    public class AdminCurrencyController : ControllerBase
    {
        private readonly IAdminCurrencyService _adminCurrencyService;
        private readonly ICurrencyRateService _currencyRateService;

        public AdminCurrencyController(IAdminCurrencyService adminCurrencyService, ICurrencyRateService currencyRateService)
        {
            _adminCurrencyService = adminCurrencyService;
            _currencyRateService = currencyRateService;
        }

        [HttpPost]
        public async Task<IActionResult> AddCurrencyAsync(AdminAddCurrencyRequest request)
        {
            try
            {
                var currency = await _adminCurrencyService.AddCurrencyAsync(request.Code, request.Name, request.Symbol, request.Decimals);
                return StatusCode(201, currency);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ErrorResponse { Message = ex.Message });
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(CurrencyDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateCurrencyAsync(int id, AdminUpdateCurrencyRequest request)
        {
            try
            {
                var currency = await _adminCurrencyService.UpdateCurrencyAsync(id, request.Name, request.Symbol, request.Decimals);
                return Ok(currency);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ErrorResponse { Message = "CURRENCY_NOT_FOUND" });
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteCurrencyAsync(int id)
        {
            try
            {
                await _adminCurrencyService.DeleteCurrencyAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ErrorResponse { Message = "CURRENCY_NOT_FOUND" });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ErrorResponse { Message = ex.Message });
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        [HttpGet("{id:int}/defaults")]
        [ProducesResponseType(typeof(IEnumerable<CurrencyDefaultRateDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetCurrencyDefaultsAsync(int id)
        {
            try
            {
                var defaults = await _adminCurrencyService.GetCurrencyDefaultsAsync(id);
                return Ok(defaults);
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        [HttpPost("defaults")]
        public async Task<IActionResult> SetDefaultRateAsync(SetDefaultRateRequest request)
        {
            var userId = JwtCookieReader.GetUserId(Request);
            if (userId is null)
                return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

            try
            {
                await _currencyRateService.SetDefaultFallbackAsync(request, userId.Value);
                return NoContent();
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }
    }
}
