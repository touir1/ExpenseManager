using Microsoft.AspNetCore.Http;
using Touir.ExpensesManager.Expenses.Infrastructure;

namespace Touir.ExpensesManager.Expenses.Tests.Infrastructure
{
    public class JwtCookieReaderIsAdminTests
    {
        private static HttpRequest MakeRequestWithCookie(string? cookieValue)
        {
            var ctx = new DefaultHttpContext();
            if (cookieValue is not null)
                ctx.Request.Headers.Cookie = $"auth_token={cookieValue}";
            return ctx.Request;
        }

        private static HttpRequest MakeRequestWithBearer(string token)
        {
            var ctx = new DefaultHttpContext();
            ctx.Request.Headers.Authorization = $"Bearer {token}";
            return ctx.Request;
        }

        private static string BuildJwt(string payloadJson)
        {
            var b64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payloadJson))
                .TrimEnd('=').Replace('+', '-').Replace('/', '_');
            return $"eyJhbGciOiJIUzI1NiJ9.{b64}.sig";
        }

        // isAdmin="true" string value
        private static readonly string AdminStringJwt =
            BuildJwt("{\"sub\":\"1\",\"isAdmin\":\"true\"}");

        // isAdmin=true boolean value
        private static readonly string AdminBoolJwt =
            BuildJwt("{\"sub\":\"1\",\"isAdmin\":true}");

        // isAdmin="false"
        private static readonly string NotAdminStringJwt =
            BuildJwt("{\"sub\":\"1\",\"isAdmin\":\"false\"}");

        // no isAdmin claim
        private static readonly string NoAdminClaimJwt =
            BuildJwt("{\"sub\":\"1\"}");

        [Fact]
        public void GetIsAdmin_ReturnsTrue_WhenIsAdminStringTrue()
        {
            Assert.True(JwtCookieReader.GetIsAdmin(MakeRequestWithCookie(AdminStringJwt)));
        }

        [Fact]
        public void GetIsAdmin_ReturnsTrue_WhenIsAdminBoolTrue()
        {
            Assert.True(JwtCookieReader.GetIsAdmin(MakeRequestWithCookie(AdminBoolJwt)));
        }

        [Fact]
        public void GetIsAdmin_ReturnsFalse_WhenIsAdminStringFalse()
        {
            Assert.False(JwtCookieReader.GetIsAdmin(MakeRequestWithCookie(NotAdminStringJwt)));
        }

        [Fact]
        public void GetIsAdmin_ReturnsFalse_WhenIsAdminClaimMissing()
        {
            Assert.False(JwtCookieReader.GetIsAdmin(MakeRequestWithCookie(NoAdminClaimJwt)));
        }

        [Fact]
        public void GetIsAdmin_ReturnsFalse_WhenNoCookie()
        {
            Assert.False(JwtCookieReader.GetIsAdmin(MakeRequestWithCookie(null)));
        }

        [Fact]
        public void GetIsAdmin_ReturnsTrue_WhenBearerHeaderUsed()
        {
            Assert.True(JwtCookieReader.GetIsAdmin(MakeRequestWithBearer(AdminStringJwt)));
        }

        [Fact]
        public void GetIsAdmin_ReturnsFalse_WhenInvalidJwtStructure()
        {
            Assert.False(JwtCookieReader.GetIsAdmin(MakeRequestWithCookie("not.a.valid.jwt.atall")));
        }
    }
}
