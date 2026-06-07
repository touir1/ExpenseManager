using Microsoft.AspNetCore.Http;
using Touir.ExpensesManager.Notifications.Infrastructure;

namespace Touir.ExpensesManager.Notifications.Tests.Infrastructure
{
    public class JwtCookieReaderTests
    {
        private static HttpRequest MakeRequest(string? cookieValue = null, string? bearerToken = null)
        {
            var ctx = new DefaultHttpContext();
            if (cookieValue is not null)
                ctx.Request.Headers.Cookie = $"auth_token={cookieValue}";
            if (bearerToken is not null)
                ctx.Request.Headers.Authorization = $"Bearer {bearerToken}";
            return ctx.Request;
        }

        // sub=42, exp=9999999999 — valid HS256 structure, no sig validation needed
        private const string ValidJwt =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9" +
            ".eyJzdWIiOiI0MiIsImV4cCI6OTk5OTk5OTk5OX0" +
            ".placeholder";

        [Fact]
        public void GetUserId_ReturnsSub_WhenValidCookie()
        {
            var result = JwtCookieReader.GetUserId(MakeRequest(ValidJwt));
            Assert.Equal(42, result);
        }

        [Fact]
        public void GetUserId_ReturnsSub_WhenBearerHeader()
        {
            var result = JwtCookieReader.GetUserId(MakeRequest(bearerToken: ValidJwt));
            Assert.Equal(42, result);
        }

        [Fact]
        public void GetUserId_ReturnsNull_WhenNoCookie()
        {
            var result = JwtCookieReader.GetUserId(MakeRequest());
            Assert.Null(result);
        }

        [Fact]
        public void GetUserId_ReturnsNull_WhenCookieEmpty()
        {
            var result = JwtCookieReader.GetUserId(MakeRequest("   "));
            Assert.Null(result);
        }

        [Fact]
        public void GetUserId_ReturnsNull_WhenWrongPartCount()
        {
            var result = JwtCookieReader.GetUserId(MakeRequest("header.payload"));
            Assert.Null(result);
        }

        [Fact]
        public void GetUserId_ReturnsNull_WhenPayloadNotValidBase64()
        {
            var result = JwtCookieReader.GetUserId(MakeRequest("header.!!!.sig"));
            Assert.Null(result);
        }

        [Fact]
        public void GetUserId_ReturnsNull_WhenSubMissing()
        {
            var payloadJson = "{\"exp\":9999999999}";
            var payloadB64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payloadJson))
                .TrimEnd('=').Replace('+', '-').Replace('/', '_');
            var jwt = $"eyJhbGciOiJIUzI1NiJ9.{payloadB64}.sig";
            var result = JwtCookieReader.GetUserId(MakeRequest(jwt));
            Assert.Null(result);
        }

        [Fact]
        public void GetUserId_ReturnsNull_WhenSubNotInteger()
        {
            var payloadJson = "{\"sub\":\"not-a-number\"}";
            var payloadB64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payloadJson))
                .TrimEnd('=').Replace('+', '-').Replace('/', '_');
            var jwt = $"eyJhbGciOiJIUzI1NiJ9.{payloadB64}.sig";
            var result = JwtCookieReader.GetUserId(MakeRequest(jwt));
            Assert.Null(result);
        }
    }
}
