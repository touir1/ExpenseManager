using System.Text;
using System.Text.Json;
using Touir.ExpensesManager.Users.Controllers.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Touir.ExpensesManager.Users.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AdminAuthorizeAttribute : Attribute, IActionFilter
    {
        private const string AuthCookie = "auth_token";

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!IsAdmin(context.HttpContext.Request))
                context.Result = new ObjectResult(new ErrorResponse { Message = "FORBIDDEN" }) { StatusCode = 403 };
        }

        public void OnActionExecuted(ActionExecutedContext context) { }

        private static bool IsAdmin(HttpRequest request)
        {
            string? token = null;

            if (request.Cookies.TryGetValue(AuthCookie, out var cookie) && !string.IsNullOrWhiteSpace(cookie))
                token = cookie;
            else
            {
                var header = request.Headers.Authorization.ToString();
                if (header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    token = header["Bearer ".Length..].Trim();
            }

            if (token is null) return false;

            try
            {
                var parts = token.Split('.');
                if (parts.Length != 3) return false;

                var payload = parts[1].Replace('-', '+').Replace('_', '/');
                var remainder = payload.Length % 4;
                if (remainder == 2) payload += "==";
                else if (remainder == 3) payload += "=";

                var bytes = Convert.FromBase64String(payload);
                var json = Encoding.UTF8.GetString(bytes);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("isAdmin", out var isAdmin))
                {
                    if (isAdmin.ValueKind == JsonValueKind.True) return true;
                    if (isAdmin.ValueKind == JsonValueKind.String) return isAdmin.GetString() == "true";
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
