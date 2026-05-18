using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Responses;
using Touir.ExpensesManager.Expenses.Infrastructure;
using Touir.ExpensesManager.Expenses.Services;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Controllers
{
    [Route("dashboard")]
    [ApiController]
    [EnableRateLimiting("expenses_global")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        /// <summary>
        /// Returns a summary of expenses for the period: total, delta vs. previous period, top category, count.
        /// </summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetSummaryAsync(
            [FromQuery] int? familyId,
            [FromQuery] DateOnly? dateFrom,
            [FromQuery] DateOnly? dateTo,
            [FromQuery] int? displayCurrencyId)
        {
            try
            {
                var userId = JwtCookieReader.GetUserId(Request);
                if (userId is null)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

                var from = dateFrom ?? new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var to = dateTo ?? DateOnly.FromDateTime(DateTime.UtcNow);

                var dto = await _dashboardService.GetSummaryAsync(userId.Value, familyId, from, to, displayCurrencyId);
                return Ok(dto);
            }
            catch (FamilyForbiddenException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse { Message = ex.Message });
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>
        /// Returns monthly expense totals broken down by category.
        /// </summary>
        [HttpGet("monthly")]
        [ProducesResponseType(typeof(IEnumerable<MonthlyBreakdownDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetMonthlyAsync(
            [FromQuery] int? familyId,
            [FromQuery] DateOnly? dateFrom,
            [FromQuery] DateOnly? dateTo,
            [FromQuery] int? displayCurrencyId)
        {
            try
            {
                var userId = JwtCookieReader.GetUserId(Request);
                if (userId is null)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

                var from = dateFrom ?? new DateOnly(DateTime.UtcNow.Year, 1, 1);
                var to = dateTo ?? DateOnly.FromDateTime(DateTime.UtcNow);

                var dto = await _dashboardService.GetMonthlyAsync(userId.Value, familyId, from, to, displayCurrencyId);
                return Ok(dto);
            }
            catch (FamilyForbiddenException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse { Message = ex.Message });
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>
        /// Returns category and subcategory breakdown for the period.
        /// </summary>
        [HttpGet("categories")]
        [ProducesResponseType(typeof(IEnumerable<CategoryBreakdownDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetCategoriesAsync(
            [FromQuery] int? familyId,
            [FromQuery] DateOnly? dateFrom,
            [FromQuery] DateOnly? dateTo,
            [FromQuery] int? displayCurrencyId)
        {
            try
            {
                var userId = JwtCookieReader.GetUserId(Request);
                if (userId is null)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

                var from = dateFrom ?? new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var to = dateTo ?? DateOnly.FromDateTime(DateTime.UtcNow);

                var dto = await _dashboardService.GetCategoriesAsync(userId.Value, familyId, from, to, displayCurrencyId);
                return Ok(dto);
            }
            catch (FamilyForbiddenException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse { Message = ex.Message });
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>
        /// Returns totals for a given calendar month across all available years.
        /// </summary>
        [HttpGet("same-month-across-years")]
        [ProducesResponseType(typeof(IEnumerable<SameMonthYearlyDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetSameMonthAcrossYearsAsync(
            [FromQuery] int? month,
            [FromQuery] int? familyId,
            [FromQuery] int? displayCurrencyId)
        {
            try
            {
                var userId = JwtCookieReader.GetUserId(Request);
                if (userId is null)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

                int targetMonth = month ?? DateTime.UtcNow.Month;
                if (targetMonth < 1 || targetMonth > 12)
                    return BadRequest(new ErrorResponse { Message = ControllerErrors.InvalidMonth });

                var dto = await _dashboardService.GetSameMonthAcrossYearsAsync(userId.Value, familyId, targetMonth, displayCurrencyId);
                return Ok(dto);
            }
            catch (FamilyForbiddenException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse { Message = ex.Message });
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>
        /// Returns per-currency expense totals for the period.
        /// </summary>
        [HttpGet("by-currency")]
        [ProducesResponseType(typeof(IEnumerable<CurrencyBreakdownDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByCurrencyAsync(
            [FromQuery] int? familyId,
            [FromQuery] DateOnly? dateFrom,
            [FromQuery] DateOnly? dateTo,
            [FromQuery] int? displayCurrencyId)
        {
            try
            {
                var userId = JwtCookieReader.GetUserId(Request);
                if (userId is null)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

                var from = dateFrom ?? new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                var to = dateTo ?? DateOnly.FromDateTime(DateTime.UtcNow);

                var dto = await _dashboardService.GetByCurrencyAsync(userId.Value, familyId, from, to, displayCurrencyId);
                return Ok(dto);
            }
            catch (FamilyForbiddenException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse { Message = ex.Message });
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>
        /// Returns the 10 most recent expenses.
        /// </summary>
        [HttpGet("recent")]
        [ProducesResponseType(typeof(ExpensePagedResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetRecentAsync(
            [FromQuery] int? familyId,
            [FromQuery] DateOnly? dateFrom,
            [FromQuery] DateOnly? dateTo)
        {
            try
            {
                var userId = JwtCookieReader.GetUserId(Request);
                if (userId is null)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

                var dto = await _dashboardService.GetRecentAsync(userId.Value, familyId, dateFrom, dateTo);
                return Ok(dto);
            }
            catch (FamilyForbiddenException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse { Message = ex.Message });
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }
    }
}
