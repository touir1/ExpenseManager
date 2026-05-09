using System.Text;
using System.Text.Json;

namespace Touir.ExpensesManager.Expenses.Infrastructure
{
    public static class JwtCookieReader
    {
        private const string AuthCookie = "auth_token";

        // nginx already validated the token; decode payload to extract sub claim
        public static int? GetUserId(HttpRequest request)
        {
            if (!request.Cookies.TryGetValue(AuthCookie, out var token) || string.IsNullOrWhiteSpace(token))
                return null;

            try
            {
                var parts = token.Split('.');
                if (parts.Length != 3)
                    return null;

                var payload = parts[1].Replace('-', '+').Replace('_', '/');
                var remainder = payload.Length % 4;
                if (remainder == 2) payload += "==";
                else if (remainder == 3) payload += "=";

                var bytes = Convert.FromBase64String(payload);
                var json = Encoding.UTF8.GetString(bytes);

                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("sub", out var sub) && int.TryParse(sub.GetString(), out var id))
                    return id;

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
