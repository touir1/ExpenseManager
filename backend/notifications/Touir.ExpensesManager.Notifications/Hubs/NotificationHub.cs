using Microsoft.AspNetCore.SignalR;
using System.Diagnostics.CodeAnalysis;
using Touir.ExpensesManager.Notifications.Infrastructure;

namespace Touir.ExpensesManager.Notifications.Hubs
{
    [ExcludeFromCodeCoverage]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext is null)
            {
                Context.Abort();
                return;
            }

            var userId = JwtCookieReader.GetUserId(httpContext.Request);
            if (userId is null)
            {
                Context.Abort();
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, userId.Value.ToString());
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var httpContext = Context.GetHttpContext();
            if (httpContext is not null)
            {
                var userId = JwtCookieReader.GetUserId(httpContext.Request);
                if (userId is not null)
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId.Value.ToString());
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
