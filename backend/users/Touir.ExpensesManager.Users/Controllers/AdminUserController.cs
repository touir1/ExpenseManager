using Touir.ExpensesManager.Users.Controllers.DTO;
using Touir.ExpensesManager.Users.Controllers.Requests;
using Touir.ExpensesManager.Users.Controllers.Responses;
using Touir.ExpensesManager.Users.Filters;
using Touir.ExpensesManager.Users.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.IdentityModel.Tokens.Jwt;

namespace Touir.ExpensesManager.Users.Controllers
{
    [Route("admin/users")]
    [ApiController]
    [AdminAuthorize]
    public class AdminUserController : ControllerBase
    {
        private const string AuthTokenCookie = "auth_token";
        private readonly IAdminUserService _adminUserService;

        public AdminUserController(IAdminUserService adminUserService)
        {
            _adminUserService = adminUserService;
        }

        private int? GetUserId()
        {
            if (!Request.Cookies.TryGetValue(AuthTokenCookie, out var token) || string.IsNullOrWhiteSpace(token))
            {
                var header = Request.Headers.Authorization.ToString();
                if (header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    token = header["Bearer ".Length..].Trim();
            }
            if (string.IsNullOrWhiteSpace(token)) return null;
            try
            {
                var parts = token.Split('.');
                if (parts.Length < 2) return null;
                var padded = parts[1].PadRight(parts[1].Length + (4 - parts[1].Length % 4) % 4, '=');
                var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(padded));
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("sub", out var sub))
                {
                    if (sub.ValueKind == System.Text.Json.JsonValueKind.Number && sub.TryGetInt32(out var numId)) return numId;
                    if (sub.ValueKind == System.Text.Json.JsonValueKind.String && int.TryParse(sub.GetString(), out var strId)) return strId;
                }
                return null;
            }
            catch { return null; }
        }

        [HttpGet]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetUsersAsync([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var (users, total) = await _adminUserService.GetUsersPagedAsync(search, page, pageSize);
                return Ok(new { Users = users, Total = total, Page = page, PageSize = pageSize });
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        [HttpGet("roles")]
        [ProducesResponseType(typeof(IEnumerable<RoleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetRolesAsync()
        {
            try
            {
                var roles = await _adminUserService.GetAllRolesAsync();
                return Ok(roles);
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        [HttpPatch("{id:int}/disable")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DisableUserAsync(int id)
        {
            var adminId = GetUserId();
            if (adminId == id)
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse { Message = ControllerErrors.CannotSelfDisable });

            try
            {
                var result = await _adminUserService.DisableUserAsync(id);
                if (!result)
                    return NotFound(new ErrorResponse { Message = ControllerErrors.UserNotFound });
                return NoContent();
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        [HttpPatch("{id:int}/enable")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EnableUserAsync(int id)
        {
            try
            {
                var result = await _adminUserService.EnableUserAsync(id);
                if (!result)
                    return NotFound(new ErrorResponse { Message = ControllerErrors.UserNotFound });
                return NoContent();
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }

        [HttpPut("{id:int}/roles")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SetUserRolesAsync(int id, SetUserRolesRequest request)
        {
            var adminId = GetUserId();
            if (adminId is null)
                return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingToken });

            try
            {
                await _adminUserService.SetUserRolesAsync(id, request.RoleIds, adminId.Value);
                return NoContent();
            }
            catch (InvalidOperationException ex) when (ex.Message == ControllerErrors.CannotRemoveOwnAdminRole)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse { Message = ControllerErrors.CannotRemoveOwnAdminRole });
            }
            catch (Exception)
            {
                return BadRequest(new ErrorResponse { Message = ControllerErrors.ServerError });
            }
        }
    }
}
