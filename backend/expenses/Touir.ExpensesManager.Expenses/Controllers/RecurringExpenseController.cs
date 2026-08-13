using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Requests;
using Touir.ExpensesManager.Expenses.Controllers.Responses;
using Touir.ExpensesManager.Expenses.Infrastructure;
using Touir.ExpensesManager.Expenses.Services;
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

        /// <summary>
        /// Return all recurring expense templates owned by the authenticated user.
        /// </summary>
        /// <param name="includeInactive">Whether to include paused (IsActive=false) templates. Default false.</param>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<RecurringExpenseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllAsync([FromQuery] bool includeInactive = false)
        {
            try
            {
                var userId = JwtCookieReader.GetUserId(Request);
                if (userId is null)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

                var list = await _recurringExpenseService.GetAllAsync(userId.Value, includeInactive);
                return Ok(list);
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>
        /// Get a single recurring expense template by ID. Only returns templates owned by the authenticated user.
        /// </summary>
        /// <param name="id">Recurring expense template ID.</param>
        [HttpGet("{id:int}", Name = "GetRecurringExpenseById")]
        [ProducesResponseType(typeof(RecurringExpenseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            try
            {
                var userId = JwtCookieReader.GetUserId(Request);
                if (userId is null)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

                var dto = await _recurringExpenseService.GetByIdAsync(id, userId.Value);
                if (dto is null)
                    return NotFound(new ErrorResponse { Message = ControllerErrors.RecurringExpenseNotFound });

                return Ok(dto);
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>
        /// Create a new recurring expense template for the authenticated user.
        /// </summary>
        /// <param name="request">Recurring expense template details.</param>
        [HttpPost]
        [ProducesResponseType(typeof(RecurringExpenseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAsync(CreateRecurringExpenseRequest request)
        {
            try
            {
                var userId = JwtCookieReader.GetUserId(Request);
                if (userId is null)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

                var dto = await _recurringExpenseService.CreateAsync(request, userId.Value);
                return CreatedAtRoute("GetRecurringExpenseById", new { id = dto.Id }, dto);
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>
        /// Update an existing recurring expense template owned by the authenticated user.
        /// </summary>
        /// <param name="id">Recurring expense template ID.</param>
        /// <param name="request">Updated template fields.</param>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(RecurringExpenseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateAsync(int id, UpdateRecurringExpenseRequest request)
        {
            try
            {
                var userId = JwtCookieReader.GetUserId(Request);
                if (userId is null)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

                var dto = await _recurringExpenseService.UpdateAsync(id, request, userId.Value);
                if (dto is null)
                    return NotFound(new ErrorResponse { Message = ControllerErrors.RecurringExpenseNotFound });

                return Ok(dto);
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>
        /// Soft-delete a recurring expense template owned by the authenticated user.
        /// </summary>
        /// <param name="id">Recurring expense template ID.</param>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            try
            {
                var userId = JwtCookieReader.GetUserId(Request);
                if (userId is null)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

                var deleted = await _recurringExpenseService.DeleteAsync(id, userId.Value);
                if (!deleted)
                    return NotFound(new ErrorResponse { Message = ControllerErrors.RecurringExpenseNotFound });

                return NoContent();
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>
        /// Confirm a due recurring expense template, creating the real expense and advancing the template's next due date.
        /// </summary>
        /// <param name="id">Recurring expense template ID.</param>
        [HttpPost("{id:int}/confirm")]
        [ProducesResponseType(typeof(ExpenseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfirmAsync(int id)
        {
            try
            {
                var userId = JwtCookieReader.GetUserId(Request);
                if (userId is null)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

                var dto = await _recurringExpenseService.ConfirmAsync(id, userId.Value);
                if (dto is null)
                    return NotFound(new ErrorResponse { Message = ControllerErrors.RecurringExpenseNotFound });

                return Ok(dto);
            }
            catch (RecurringExpenseNotDueException ex)
            {
                return BadRequest(new ErrorResponse { Message = ex.Message });
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
