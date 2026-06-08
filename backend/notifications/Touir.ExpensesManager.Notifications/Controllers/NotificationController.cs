using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.Json;
using Touir.ExpensesManager.Notifications.Controllers.DTO;
using Touir.ExpensesManager.Notifications.Controllers.Requests;
using Touir.ExpensesManager.Notifications.Controllers.Responses;
using Touir.ExpensesManager.Notifications.Infrastructure;
using Touir.ExpensesManager.Notifications.Services.Contracts;

namespace Touir.ExpensesManager.Notifications.Controllers
{
    [Route("")]
    [ApiController]
    [EnableRateLimiting("notifications_global")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Return a paginated list of notifications for the authenticated user, newest first.
        /// </summary>
        /// <param name="page">Page number (1-based, default 1).</param>
        /// <param name="pageSize">Items per page (default 20).</param>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<NotificationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = JwtCookieReader.GetUserId(Request);
            if (userId is null)
                return Unauthorized(new ErrorResponse { Message = "UNAUTHORIZED" });

            try
            {
                var notifications = await _notificationService.GetNotificationsAsync(userId.Value, page, pageSize);
                var dtos = notifications.Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Type = n.Type,
                    Payload = JsonSerializer.Deserialize<JsonElement>(n.Payload),
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    ReadAt = n.ReadAt
                });
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return BadRequest(new ErrorResponse { Message = "SERVER_ERROR" });
            }
        }

        /// <summary>
        /// Return the count of unread notifications for the authenticated user. Used to populate the bell badge.
        /// </summary>
        [HttpGet("unread-count")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = JwtCookieReader.GetUserId(Request);
            if (userId is null)
                return Unauthorized(new ErrorResponse { Message = "UNAUTHORIZED" });

            try
            {
                var count = await _notificationService.GetUnreadCountAsync(userId.Value);
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return BadRequest(new ErrorResponse { Message = "SERVER_ERROR" });
            }
        }

        /// <summary>
        /// Mark a single notification as read. No-op if already read. Returns 204 regardless of whether the notification exists for the user.
        /// </summary>
        /// <param name="id">Notification ID.</param>
        [HttpPost("{id:long}/read")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> MarkAsRead(long id)
        {
            var userId = JwtCookieReader.GetUserId(Request);
            if (userId is null)
                return Unauthorized(new ErrorResponse { Message = "UNAUTHORIZED" });

            try
            {
                await _notificationService.MarkAsReadAsync(id, userId.Value);
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return BadRequest(new ErrorResponse { Message = "SERVER_ERROR" });
            }
        }

        /// <summary>
        /// Mark all notifications for the authenticated user as read. No-op if none are unread.
        /// </summary>
        [HttpPost("read-all")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = JwtCookieReader.GetUserId(Request);
            if (userId is null)
                return Unauthorized(new ErrorResponse { Message = "UNAUTHORIZED" });

            try
            {
                await _notificationService.MarkAllAsReadAsync(userId.Value);
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return BadRequest(new ErrorResponse { Message = "SERVER_ERROR" });
            }
        }

        /// <summary>
        /// Register a device push token for the authenticated user.
        /// Stub — token is accepted and acknowledged; FCM/APNs dispatch is deferred to Phase 15.
        /// </summary>
        [HttpPost("push-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public IActionResult RegisterPushToken([FromBody] RegisterPushTokenRequest request)
        {
            var userId = JwtCookieReader.GetUserId(Request);
            if (userId is null)
                return Unauthorized(new ErrorResponse { Message = "UNAUTHORIZED" });

            // Phase 15: persist token to PushTokens table and integrate with FCM/APNs.
            return Ok();
        }
    }
}
