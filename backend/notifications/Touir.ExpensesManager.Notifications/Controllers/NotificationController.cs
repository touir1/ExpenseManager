using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.Json;
using Touir.ExpensesManager.Notifications.Controllers.DTO;
using Touir.ExpensesManager.Notifications.Controllers.Responses;
using Touir.ExpensesManager.Notifications.Infrastructure;
using Touir.ExpensesManager.Notifications.Services.Contracts;

namespace Touir.ExpensesManager.Notifications.Controllers
{
    [Route("notifications")]
    [ApiController]
    [EnableRateLimiting("notifications_global")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
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

        [HttpGet("unread-count")]
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

        [HttpPost("{id:long}/read")]
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

        [HttpPost("read-all")]
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
    }
}
