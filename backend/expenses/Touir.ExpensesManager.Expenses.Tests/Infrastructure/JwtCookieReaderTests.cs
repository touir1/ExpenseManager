using Microsoft.AspNetCore.Http;
using Touir.ExpensesManager.Expenses.Infrastructure;

namespace Touir.ExpensesManager.Expenses.Tests.Infrastructure
{
    public class JwtCookieReaderTests
    {
        private static HttpRequest MakeRequest(string? cookieValue = null)
        {
            var ctx = new DefaultHttpContext();
            if (cookieValue is not null)
                ctx.Request.Headers.Cookie = $"auth_token={cookieValue}";
            return ctx.Request;
        }

        // sub=42, exp=9999999999 — valid HS256 structure, no sig validation needed
        private const string ValidJwt =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9" +
            ".eyJzdWIiOiI0MiIsImV4cCI6OTk5OTk5OTk5OX0" +
            ".placeholder";

        [Fact]
        public void GetUserId_ReturnsSub_WhenValidJwt()
        {
            var result = JwtCookieReader.GetUserId(MakeRequest(ValidJwt));
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
            // only 2 parts — not a valid JWT structure
            var result = JwtCookieReader.GetUserId(MakeRequest("header.payload"));
            Assert.Null(result);
        }

        [Fact]
        public void GetUserId_ReturnsNull_WhenPayloadNotValidBase64()
        {
            // 3 parts but payload is garbage — triggers catch block
            var result = JwtCookieReader.GetUserId(MakeRequest("header.!!!.sig"));
            Assert.Null(result);
        }

        [Fact]
        public void GetUserId_ReturnsNull_WhenSubMissing()
        {
            // payload = {"exp":9999999999} — no sub claim
            // base64url of {"exp":9999999999}
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
            // payload = {"sub":"not-a-number"}
            var payloadJson = "{\"sub\":\"not-a-number\"}";
            var payloadB64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payloadJson))
                .TrimEnd('=').Replace('+', '-').Replace('/', '_');
            var jwt = $"eyJhbGciOiJIUzI1NiJ9.{payloadB64}.sig";
            var result = JwtCookieReader.GetUserId(MakeRequest(jwt));
            Assert.Null(result);
        }
    }
}
