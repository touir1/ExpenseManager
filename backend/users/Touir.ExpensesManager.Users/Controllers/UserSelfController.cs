using System.Text.Json;
using Touir.ExpensesManager.Users.Controllers.Responses;
using Touir.ExpensesManager.Users.Messaging.Messages;
using Touir.ExpensesManager.Users.Models;
using Touir.ExpensesManager.Users.Repositories.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Touir.ExpensesManager.Users.Controllers
{
    [Route("me")]
    [ApiController]
    public class UserSelfController : ControllerBase
    {
        private const string AuthTokenCookie = "auth_token";

        private readonly IUserRepository _userRepository;
        private readonly IOutboxRepository _outboxRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public UserSelfController(
            IUserRepository userRepository,
            IOutboxRepository outboxRepository,
            IRefreshTokenRepository refreshTokenRepository)
        {
            _userRepository = userRepository;
            _outboxRepository = outboxRepository;
            _refreshTokenRepository = refreshTokenRepository;
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
        /// Soft-delete the authenticated user's account and publish a user.deleted event.
        /// </summary>
        [HttpDelete]
        [EnableRateLimiting("change_password")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAccountAsync()
        {
            var userId = GetUserId();
            if (userId is null)
                return Unauthorized(new ErrorResponse { Message = ControllerErrors.MissingToken });

            var user = await _userRepository.GetUserByIdAsync(userId.Value);
            if (user is null)
                return NotFound(new ErrorResponse { Message = ControllerErrors.UserNotFound });

            await _userRepository.DeleteUserAsync(user);

            await _refreshTokenRepository.RevokeAllByUserIdAsync(userId.Value);

            await _outboxRepository.EnqueueAsync(new OutboxEvent
            {
                MessageId = Guid.NewGuid().ToString(),
                EventType = UserEventType.Deleted,
                Payload = JsonSerializer.Serialize(new UserEventMessage
                {
                    EventType = UserEventType.Deleted,
                    UserId = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                }),
                CreatedAt = DateTime.UtcNow,
                RetryCount = 0
            });

            Response.Cookies.Delete(AuthTokenCookie);
            Response.Cookies.Delete("refresh_token");

            return NoContent();
        }
    }
}
