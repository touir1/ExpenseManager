using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Requests;
using Touir.ExpensesManager.Expenses.Controllers.Responses;
using Touir.ExpensesManager.Expenses.Services.Contracts;
using Touir.ExpensesManager.Expenses.Infrastructure;

namespace Touir.ExpensesManager.Expenses.Controllers
{
    [ApiController]
    [Route("tags")]
    [EnableRateLimiting("expenses_global")]
    public class TagController : ControllerBase
    {
        private readonly ITagService _tagService;

        public TagController(ITagService tagService)
        {
            _tagService = tagService;
        }

        /// <summary>
        /// Return all tags visible to the authenticated user, split into own (adopted by user) and family (adopted by co-members of shared families).
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(TagListDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTagsAsync()
        {
            var userId = JwtCookieReader.GetUserId(Request);
            if (userId is null)
                return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

            var tags = await _tagService.GetVisibleAsync(userId.Value);
            return Ok(tags);
        }

        /// <summary>
        /// Create or adopt a tag by name (idempotent, case-sensitive). Returns the existing tag if the name already exists.
        /// Attaches the tag to the authenticated user (creates a UserTag row if not already present).
        /// </summary>
        /// <param name="request">Tag name (max 50 characters).</param>
        [HttpPost]
        [ProducesResponseType(typeof(TagDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UseTagAsync([FromBody] CreateTagRequest request)
        {
            var userId = JwtCookieReader.GetUserId(Request);
            if (userId is null)
                return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

            var tag = await _tagService.UseTagAsync(request.Name, userId.Value);
            return Ok(tag);
        }

        /// <summary>
        /// Remove the authenticated user's adoption of a tag. The tag entity and any expense history are preserved.
        /// Returns 404 if the user has not adopted this tag.
        /// </summary>
        /// <param name="id">Tag ID.</param>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RemoveTagAsync(int id)
        {
            var userId = JwtCookieReader.GetUserId(Request);
            if (userId is null)
                return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

            var removed = await _tagService.RemoveTagAsync(id, userId.Value);
            if (!removed)
                return NotFound(new ErrorResponse { Message = ControllerErrors.TagNotFound });

            return NoContent();
        }
    }
}
