using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Responses;
using Touir.ExpensesManager.Expenses.Infrastructure;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Controllers
{
    [ApiController]
    [Route("recurring-expenses")]
    [EnableRateLimiting("expenses_global")]
    public class RecurringExpenseController : ControllerBase
    {
        private readonly IRecurringExpenseService _recurringExpenseService;

        public RecurringExpenseController(IRecurringExpenseService recurringExpenseService)
        {
            _recurringExpenseService = recurringExpenseService;
        }

        /// <summary>
        /// Return the authenticated user's next upcoming active recurring expenses, sorted by due date ascending.
        /// </summary>
        /// <param name="take">Maximum number of items to return (default 5).</param>
        [HttpGet("upcoming")]
        [ProducesResponseType(typeof(IEnumerable<RecurringExpenseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUpcomingAsync([FromQuery] int take = 5)
        {
            var userId = JwtCookieReader.GetUserId(Request);
            if (userId is null)
                return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

            var boundedTake = Math.Clamp(take, 1, 20);
            var upcoming = await _recurringExpenseService.GetUpcomingAsync(userId.Value, boundedTake);
            return Ok(upcoming);
        }
    }
}
