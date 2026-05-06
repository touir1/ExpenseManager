using Microsoft.AspNetCore.Mvc;
using Touir.ExpensesManager.Users.Repositories.Contracts;

namespace Touir.ExpensesManager.Users.Controllers
{
    [ApiController]
    [Route("messaging")]
    public class MessagingController : ControllerBase
    {
        private readonly IOutboxRepository _outboxRepository;

        public MessagingController(IOutboxRepository outboxRepository)
        {
            _outboxRepository = outboxRepository;
        }

        /// <summary>
        /// Requeue failed or undelivered outbox events for republishing.
        /// The OutboxPublisherService will pick them up within 5 seconds.
        /// </summary>
        /// <param name="eventType">Optional filter by event type (e.g. user.created).</param>
        /// <param name="from">Optional filter: only events created at or after this UTC datetime.</param>
        /// <param name="forceAll">If true, also requeue already-published events (full resync).</param>
        [HttpPost("replay")]
        public async Task<IActionResult> Replay(
            [FromQuery] string? eventType,
            [FromQuery] DateTime? from,
            [FromQuery] bool forceAll = false)
        {
            int count = await _outboxRepository.RequeueAsync(eventType, from, forceAll);
            return Ok(new { requeued = count, message = $"{count} event(s) requeued. Will be published within 5 seconds." });
        }

        /// <summary>
        /// Returns outbox statistics: pending, published, and failed event counts.
        /// </summary>
        [HttpGet("outbox/stats")]
        public async Task<IActionResult> Stats()
        {
            var pending = await _outboxRepository.GetPendingAsync(maxRetries: 5);
            return Ok(new
            {
                pending = pending.Count,
            });
        }
    }
}
