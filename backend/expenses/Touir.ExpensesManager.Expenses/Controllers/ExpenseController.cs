using Microsoft.AspNetCore.Mvc;
using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Requests;
using Touir.ExpensesManager.Expenses.Controllers.Responses;
using Touir.ExpensesManager.Expenses.Infrastructure;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Controllers
{
    [Route("expenses")]
    [ApiController]
    public class ExpenseController : ControllerBase
    {
        private const string ServerError = "SERVER_ERROR";
        private const string ExpenseNotFound = "EXPENSE_NOT_FOUND";
        private const string MissingUser = "UNAUTHORIZED";

        // OperationSource seed: 1=SingleWeb, ModifiedSource seed: 1=Web (constraints.md)
        private const int SourceSingleWeb = 1;
        private const int ModifiedSourceWeb = 1;

        private readonly IExpenseService _expenseService;

        public ExpenseController(IExpenseService expenseService)
        {
            _expenseService = expenseService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ExpenseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAsync(CreateExpenseRequest request)
        {
            try
            {
                var userId = JwtCookieReader.GetUserId(Request);
                if (userId is null)
                    return Unauthorized(new ErrorResponse { Message = MissingUser });

                var dto = await _expenseService.AddAsync(request, userId.Value, SourceSingleWeb);
                return CreatedAtAction(nameof(GetByIdAsync), new { id = dto.Id }, dto);
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ServerError });
            }
        }

        [HttpPut("{id:long}")]
        [ProducesResponseType(typeof(ExpenseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateAsync(long id, UpdateExpenseRequest request)
        {
            try
            {
                var userId = JwtCookieReader.GetUserId(Request);
                if (userId is null)
                    return Unauthorized(new ErrorResponse { Message = MissingUser });

                var dto = await _expenseService.UpdateAsync(id, request, userId.Value, ModifiedSourceWeb);
                if (dto is null)
                    return NotFound(new ErrorResponse { Message = ExpenseNotFound });

                return Ok(dto);
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ServerError });
            }
        }

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
                    return Unauthorized(new ErrorResponse { Message = MissingUser });

                var deleted = await _expenseService.DeleteAsync(id, userId.Value, SourceSingleWeb);
                if (!deleted)
                    return NotFound(new ErrorResponse { Message = ExpenseNotFound });

                return NoContent();
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ServerError });
            }
        }

        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(ExpenseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByIdAsync(long id)
        {
            try
            {
                var userId = JwtCookieReader.GetUserId(Request);
                if (userId is null)
                    return Unauthorized(new ErrorResponse { Message = MissingUser });

                var dto = await _expenseService.GetByIdAsync(id, userId.Value);
                if (dto is null)
                    return NotFound(new ErrorResponse { Message = ExpenseNotFound });

                return Ok(dto);
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ServerError });
            }
        }

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
                    return Unauthorized(new ErrorResponse { Message = MissingUser });

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
                return BadRequest(new ErrorResponse { Message = ServerError });
            }
        }
    }
}
