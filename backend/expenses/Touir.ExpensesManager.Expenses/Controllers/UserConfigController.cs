using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Requests;
using Touir.ExpensesManager.Expenses.Controllers.Responses;
using Touir.ExpensesManager.Expenses.Infrastructure;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Controllers
{
    [Route("config")]
    [ApiController]
    [EnableRateLimiting("expenses_global")]
    public class UserConfigController : ControllerBase
    {
        private readonly IUserConfigService _userConfigService;

        public UserConfigController(IUserConfigService userConfigService)
        {
            _userConfigService = userConfigService;
        }

        /// <summary>
        /// Returns the authenticated user's configuration. Returns null fields if no config row exists yet.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(UserConfigDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAsync()
        {
            var userId = JwtCookieReader.GetUserId(Request);
            if (userId is null)
                return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

            var dto = await _userConfigService.GetAsync(userId.Value);
            return Ok(dto);
        }

        /// <summary>
        /// Creates or updates the authenticated user's configuration (upsert).
        /// Pass null for DefaultCurrencyId to clear the default currency.
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(UserConfigDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateUserConfigRequest request)
        {
            var userId = JwtCookieReader.GetUserId(Request);
            if (userId is null)
                return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

            var dto = await _userConfigService.UpdateAsync(userId.Value, request.DefaultCurrencyId, request.DefaultCategoryId);
            if (dto is null)
                return BadRequest(new ErrorResponse { Message = "USER_CONFIG_INVALID_CURRENCY" });

            return Ok(dto);
        }

        /// <summary>
        /// Saves the authenticated user's default CSV column mapping (rawHeader -> canonicalField), used to
        /// auto-apply on future CSV imports whose headers match.
        /// </summary>
        [HttpPut("csv-column-mapping")]
        [ProducesResponseType(typeof(UserConfigDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateCsvColumnMappingAsync([FromBody] UpdateCsvColumnMappingRequest request)
        {
            var userId = JwtCookieReader.GetUserId(Request);
            if (userId is null)
                return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

            var dto = await _userConfigService.UpdateCsvColumnMappingAsync(userId.Value, request.Mapping);
            if (dto is null)
                return BadRequest(new ErrorResponse { Message = "INVALID_COLUMN_MAPPING" });

            return Ok(dto);
        }

        /// <summary>
        /// Clears the authenticated user's saved default CSV column mapping.
        /// </summary>
        [HttpDelete("csv-column-mapping")]
        [ProducesResponseType(typeof(UserConfigDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ClearCsvColumnMappingAsync()
        {
            var userId = JwtCookieReader.GetUserId(Request);
            if (userId is null)
                return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

            var dto = await _userConfigService.UpdateCsvColumnMappingAsync(userId.Value, null);
            return Ok(dto);
        }
    }
}
