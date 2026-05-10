using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Responses;
using Touir.ExpensesManager.Expenses.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Touir.ExpensesManager.Expenses.Controllers
{
    [Route("currencies")]
    [ApiController]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("expenses_global")]
    public class CurrencyController : ControllerBase
    {
        private const string ServerError = "SERVER_ERROR";

        private readonly ICurrencyService _currencyService;

        public CurrencyController(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        /// <summary>
        /// Return all supported currencies.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CurrencyDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                var currencies = await _currencyService.GetAllAsync();
                return Ok(currencies);
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ServerError });
            }
        }
    }
}
