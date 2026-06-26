using System.Text.Json;
using Touir.ExpensesManager.Users.Controllers.DTO;
using Touir.ExpensesManager.Users.Controllers.Requests;
using Touir.ExpensesManager.Users.Controllers.Responses;
using Touir.ExpensesManager.Users.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Touir.ExpensesManager.Users.Controllers
{
    [Route("config/notifications")]
    [ApiController]
    public class NotificationPreferencesController : ControllerBase
    {
        private const string AuthTokenCookie = "auth_token";

        private readonly INotificationPreferencesService _service;

        public NotificationPreferencesController(INotificationPreferencesService service)
        {
            _service = service;
        }

        private int? GetUserId()
        {
            if (!Request.Cookies.TryGetValue(AuthTokenCookie, out var token) || string.IsNullOrWhiteSpace(token))
                return null;
            try
            {
                var parts = token.Split('.');
                if (parts.Length < 2) return null;
                var padded = parts[1].PadRight(parts[1].Length + (4 - parts[1].Length % 4) % 4, '=');
                var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(padded));
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("sub", out var sub))
                {
                    if (sub.ValueKind == JsonValueKind.Number && sub.TryGetInt32(out var numId)) return numId;
                    if (sub.ValueKind == JsonValueKind.String && int.TryParse(sub.GetString(), out var strId)) return strId;
                }
                return null;
            }
            catch { return null; }
        }

        /// <summary>
        /// Get notification preferences for the authenticated user.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<NotificationPreferenceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAsync()
        {
            var userId = GetUserId();
            if (userId is null)
                return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingToken });

            var prefs = await _service.GetAsync(userId.Value);
            return Ok(prefs);
        }

        /// <summary>
        /// Update notification preferences for the authenticated user.
        /// </summary>
        [HttpPut]
        [EnableRateLimiting("change_password")]
        [ProducesResponseType(typeof(IReadOnlyList<NotificationPreferenceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateNotificationPreferencesRequest request)
        {
            var userId = GetUserId();
            if (userId is null)
                return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingToken });

            var result = await _service.UpdateAsync(userId.Value, request.Preferences);
            return Ok(result);
        }
    }
}
