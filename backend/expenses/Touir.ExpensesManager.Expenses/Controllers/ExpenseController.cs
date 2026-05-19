using Microsoft.AspNetCore.Mvc;
using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Requests;
using Touir.ExpensesManager.Expenses.Controllers.Responses;
using Touir.ExpensesManager.Expenses.Infrastructure;
using Touir.ExpensesManager.Expenses.Services;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Controllers
{
    [Route("")]
    [ApiController]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("expenses_global")]
    public class ExpenseController : ControllerBase
    {
        // OperationSource seed: 1=SingleWeb, ModifiedSource seed: 1=Web (constraints.md)
        private const int SourceSingleWeb = 1;
        private const int ModifiedSourceWeb = 1;

        private readonly IExpenseService _expenseService;

        public ExpenseController(IExpenseService expenseService)
        {
            _expenseService = expenseService;
        }

        /// <summary>
        /// Create a new expense for the authenticated user.
        /// </summary>
        /// <param name="request">Expense details including amount, currency, category, and date.</param>
        [HttpPost]
        [ProducesResponseType(typeof(ExpenseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAsync(CreateExpenseRequest request)
        {
            try
            {
                var userId = JwtCookieReader.GetUserId(Request);
                if (userId is null)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

                var dto = await _expenseService.AddAsync(request, userId.Value, SourceSingleWeb);
                return CreatedAtRoute("GetExpenseById", new { id = dto.Id }, dto);
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
        /// Update an existing expense owned by the authenticated user.
        /// </summary>
        /// <param name="id">Expense ID.</param>
        /// <param name="request">Updated expense fields.</param>
        [HttpPut("{id:long}")]
        [ProducesResponseType(typeof(ExpenseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateAsync(long id, UpdateExpenseRequest request)
        {
            try
            {
                var userId = JwtCookieReader.GetUserId(Request);
                if (userId is null)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

                var dto = await _expenseService.UpdateAsync(id, request, userId.Value, ModifiedSourceWeb);
                if (dto is null)
                    return NotFound(new ErrorResponse { Message = ControllerErrors.ExpenseNotFound });

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
        /// Soft-delete an expense owned by the authenticated user.
        /// </summary>
        /// <param name="id">Expense ID.</param>
        [HttpDelete("{id:long}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteAsync(long id)
        {
            try
            {
                var userId = JwtCookieReader.GetUserId(Request);
                if (userId is null)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

                var deleted = await _expenseService.DeleteAsync(id, userId.Value, SourceSingleWeb);
                if (!deleted)
                    return NotFound(new ErrorResponse { Message = ControllerErrors.ExpenseNotFound });

                return NoContent();
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>
        /// Get a single expense by ID. Only returns expenses owned by the authenticated user.
        /// </summary>
        /// <param name="id">Expense ID.</param>
        /// <param name="displayCurrencyId">Optional currency ID to convert amounts into.</param>
        [HttpGet("{id:long}", Name = "GetExpenseById")]
        [ProducesResponseType(typeof(ExpenseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByIdAsync(long id, [FromQuery] int? displayCurrencyId = null)
        {
            try
            {
                var userId = JwtCookieReader.GetUserId(Request);
                if (userId is null)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

                var dto = await _expenseService.GetByIdAsync(id, userId.Value, displayCurrencyId);
                if (dto is null)
                    return NotFound(new ErrorResponse { Message = ControllerErrors.ExpenseNotFound });

                return Ok(dto);
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>
        /// Return a paginated, filtered list of expenses for the authenticated user.
        /// </summary>
        /// <param name="filter">Pagination and filter parameters (page, pageSize, date range, category, etc.).</param>
        [HttpGet]
        [ProducesResponseType(typeof(ExpensePagedResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPagedAsync([FromQuery] ExpenseFilterDto filter)
        {
            try
            {
                var userId = JwtCookieReader.GetUserId(Request);
                if (userId is null)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

                var result = await _expenseService.GetPagedAsync(filter, userId.Value);
                return Ok(new ExpensePagedResponse
                {
                    Items = result.Items,
                    TotalCount = result.TotalCount,
                    Page = result.Page,
                    PageSize = result.PageSize,
                    TotalPages = result.TotalPages
                });
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }
    }
}
