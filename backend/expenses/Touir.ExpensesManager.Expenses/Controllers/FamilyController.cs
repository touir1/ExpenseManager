using Microsoft.AspNetCore.Mvc;
using Touir.ExpensesManager.Expenses.Controllers.DTO;
using Touir.ExpensesManager.Expenses.Controllers.Requests;
using Touir.ExpensesManager.Expenses.Controllers.Responses;
using Touir.ExpensesManager.Expenses.Infrastructure;
using Touir.ExpensesManager.Expenses.Services;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Controllers
{
    [Route("families")]
    [ApiController]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("expenses_global")]
    public class FamilyController : ControllerBase
    {
        private readonly IFamilyService _familyService;

        public FamilyController(IFamilyService familyService)
        {
            _familyService = familyService;
        }

        /// <summary>List all families the authenticated user belongs to (active and archived).</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<FamilyDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetByUserAsync()
        {
            try
            {
                var userId = JwtCookieReader.GetUserId(Request);
                if (userId is null)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

                var families = await _familyService.GetByUserAsync(userId.Value);
                return Ok(families);
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>Get a family by ID with its member list. User must be a member.</summary>
        [HttpGet("{id:int}", Name = "GetFamilyById")]
        [ProducesResponseType(typeof(FamilyDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            try
            {
                var userId = JwtCookieReader.GetUserId(Request);
                if (userId is null)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

                var dto = await _familyService.GetByIdAsync(id, userId.Value);
                return Ok(dto);
            }
            catch (FamilyNotFoundException ex)
            {
                return NotFound(new ErrorResponse { Message = ex.Message });
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

        /// <summary>Create a new family. The caller becomes the head.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(FamilyDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAsync(CreateFamilyRequest request)
        {
            try
            {
                var userId = JwtCookieReader.GetUserId(Request);
                if (userId is null)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

                var dto = await _familyService.CreateAsync(request.Name, userId.Value);
                return CreatedAtRoute("GetFamilyById", new { id = dto.Id }, dto);
            }
            catch (FamilyConflictException ex)
            {
                return Conflict(new ErrorResponse { Message = ex.Message });
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>Rename a family. Head only.</summary>
        [HttpPut("{id:int}/name")]
        [ProducesResponseType(typeof(FamilyDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RenameAsync(int id, RenameFamilyRequest request)
        {
            try
            {
                var userId = JwtCookieReader.GetUserId(Request);
                if (userId is null)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

                var dto = await _familyService.RenameAsync(id, request.Name, userId.Value);
                return Ok(dto);
            }
            catch (FamilyNotFoundException ex)
            {
                return NotFound(new ErrorResponse { Message = ex.Message });
            }
            catch (FamilyForbiddenException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse { Message = ex.Message });
            }
            catch (FamilyConflictException ex)
            {
                return Conflict(new ErrorResponse { Message = ex.Message });
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>Archive a family (soft delete). Head only. Default family cannot be archived.</summary>
        [HttpPost("{id:int}/archive")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ArchiveAsync(int id)
        {
            try
            {
                var userId = JwtCookieReader.GetUserId(Request);
                if (userId is null)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

                await _familyService.ArchiveAsync(id, userId.Value);
                return NoContent();
            }
            catch (FamilyNotFoundException ex)
            {
                return NotFound(new ErrorResponse { Message = ex.Message });
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

        /// <summary>Unarchive a family. Head only.</summary>
        [HttpPost("{id:int}/unarchive")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UnarchiveAsync(int id)
        {
            try
            {
                var userId = JwtCookieReader.GetUserId(Request);
                if (userId is null)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

                await _familyService.UnarchiveAsync(id, userId.Value);
                return NoContent();
            }
            catch (FamilyNotFoundException ex)
            {
                return NotFound(new ErrorResponse { Message = ex.Message });
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

        /// <summary>Invite a user by email to this family. Head only. Returns invitation token.</summary>
        [HttpPost("{id:int}/invite")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> InviteAsync(int id, InviteMemberRequest request)
        {
            try
            {
                var userId = JwtCookieReader.GetUserId(Request);
                if (userId is null)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

                var token = await _familyService.InviteAsync(id, request.Email, userId.Value);
                return Ok(new { token });
            }
            catch (FamilyNotFoundException ex)
            {
                return NotFound(new ErrorResponse { Message = ex.Message });
            }
            catch (FamilyForbiddenException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse { Message = ex.Message });
            }
            catch (FamilyConflictException ex)
            {
                return Conflict(new ErrorResponse { Message = ex.Message });
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>Accept a family invitation by token.</summary>
        [HttpPost("accept-invite/{token}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AcceptInviteAsync(string token)
        {
            try
            {
                var userId = JwtCookieReader.GetUserId(Request);
                if (userId is null)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

                await _familyService.AcceptInviteAsync(token, userId.Value);
                return NoContent();
            }
            catch (FamilyInvitationException ex)
            {
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
            catch (FamilyForbiddenException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse { Message = ex.Message });
            }
            catch (FamilyConflictException ex)
            {
                return Conflict(new ErrorResponse { Message = ex.Message });
            }
            catch (FamilyNotFoundException ex)
            {
                return NotFound(new ErrorResponse { Message = ex.Message });
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        /// <summary>Remove a member from a family. Head only. Removes their expense attributions for this family.</summary>
        [HttpDelete("{id:int}/members/{targetUserId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveMemberAsync(int id, int targetUserId)
        {
            try
            {
                var userId = JwtCookieReader.GetUserId(Request);
                if (userId is null)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

                await _familyService.RemoveMemberAsync(id, targetUserId, userId.Value);
                return NoContent();
            }
            catch (FamilyNotFoundException ex)
            {
                return NotFound(new ErrorResponse { Message = ex.Message });
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

        /// <summary>Leave a family. Heads must have at least one other head first.</summary>
        [HttpDelete("{id:int}/leave")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> LeaveAsync(int id)
        {
            try
            {
                var userId = JwtCookieReader.GetUserId(Request);
                if (userId is null)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

                await _familyService.LeaveAsync(id, userId.Value);
                return NoContent();
            }
            catch (FamilyNotFoundException ex)
            {
                return NotFound(new ErrorResponse { Message = ex.Message });
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

        /// <summary>List pending (not accepted, not expired) invitations for a family. Head only.</summary>
        [HttpGet("{id:int}/invitations")]
        [ProducesResponseType(typeof(IEnumerable<FamilyPendingInvitationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetPendingInvitationsAsync(int id)
        {
            try
            {
                var userId = JwtCookieReader.GetUserId(Request);
                if (userId is null)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

                var invitations = await _familyService.GetPendingInvitationsAsync(id, userId.Value);
                return Ok(invitations);
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

        /// <summary>Revoke a pending invitation. Head only. Deletes the invitation record.</summary>
        [HttpDelete("{id:int}/invitations/{token}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RevokeInvitationAsync(int id, string token)
        {
            try
            {
                var userId = JwtCookieReader.GetUserId(Request);
                if (userId is null)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

                await _familyService.RevokeInvitationAsync(id, token, userId.Value);
                return NoContent();
            }
            catch (FamilyInvitationException ex)
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

        /// <summary>Change a member's role. Head only.</summary>
        [HttpPut("{id:int}/members/{targetUserId:int}/role")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangeMemberRoleAsync(int id, int targetUserId, ChangeMemberRoleRequest request)
        {
            try
            {
                var userId = JwtCookieReader.GetUserId(Request);
                if (userId is null)
                    return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingUser });

                await _familyService.ChangeRoleAsync(id, targetUserId, request.Role, userId.Value);
                return NoContent();
            }
            catch (FamilyNotFoundException ex)
            {
                return NotFound(new ErrorResponse { Message = ex.Message });
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
