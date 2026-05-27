using System.Text;
using System.Text.Json;

namespace Touir.ExpensesManager.Expenses.Infrastructure
{
    public static class JwtCookieReader
    {
        private const string AuthCookie = "auth_token";

        // nginx already validated the token; decode payload to extract claims.
        // Falls back to Authorization: Bearer <token> header when cookie absent (Swagger).
        public static int? GetUserId(HttpRequest request)
        {
            var token = ExtractToken(request);
            if (token is null) return null;

            try
            {
                var doc = DecodePayload(token);
                if (doc is null) return null;
                using (doc)
                {
                    if (doc.RootElement.TryGetProperty("sub", out var sub) && int.TryParse(sub.GetString(), out var id))
                        return id;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public static bool GetIsAdmin(HttpRequest request)
        {
            var token = ExtractToken(request);
            if (token is null) return false;

            try
            {
                var doc = DecodePayload(token);
                if (doc is null) return false;
                using (doc)
                {
                    if (doc.RootElement.TryGetProperty("isAdmin", out var isAdmin))
                    {
                        if (isAdmin.ValueKind == JsonValueKind.True) return true;
                        if (isAdmin.ValueKind == JsonValueKind.String) return isAdmin.GetString() == "true";
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private static string? ExtractToken(HttpRequest request)
        {
            if (request.Cookies.TryGetValue(AuthCookie, out var cookie) && !string.IsNullOrWhiteSpace(cookie))
                return cookie;

            var header = request.Headers.Authorization.ToString();
            if (header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return header["Bearer ".Length..].Trim();

            return null;
        }

        private static JsonDocument? DecodePayload(string token)
        {
            var parts = token.Split('.');
            if (parts.Length != 3) return null;

            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            var remainder = payload.Length % 4;
            if (remainder == 2) payload += "==";
            else if (remainder == 3) payload += "=";

            var bytes = Convert.FromBase64String(payload);
            var json = Encoding.UTF8.GetString(bytes);
            return JsonDocument.Parse(json);
        }
    }
}
