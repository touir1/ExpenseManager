using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Touir.ExpensesManager.Expenses.Controllers.Responses;
using Touir.ExpensesManager.Expenses.Filters;

namespace Touir.ExpensesManager.Expenses.Tests.Filters
{
    public class AppAdminAttributeTests
    {
        private static string BuildJwt(string payloadJson)
        {
            var b64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payloadJson))
                .TrimEnd('=').Replace('+', '-').Replace('/', '_');
            return $"eyJhbGciOiJIUzI1NiJ9.{b64}.sig";
        }

        private static ActionExecutingContext MakeContext(string? cookieJwt = null)
        {
            var httpContext = new DefaultHttpContext();
            if (cookieJwt is not null)
                httpContext.Request.Headers.Cookie = $"auth_token={cookieJwt}";

            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            return new ActionExecutingContext(actionContext, [], new Dictionary<string, object?>(), new object());
        }

        [Fact]
        public void OnActionExecuting_Sets403_WhenNoCookie()
        {
            var filter = new AppAdminAttribute();
            var context = MakeContext(null);

            filter.OnActionExecuting(context);

            var result = Assert.IsType<ObjectResult>(context.Result);
            Assert.Equal(403, result.StatusCode);
        }

        [Fact]
        public void OnActionExecuting_Sets403_WhenIsAdminFalse()
        {
            var jwt = BuildJwt("{\"sub\":\"1\",\"isAdmin\":\"false\"}");
            var filter = new AppAdminAttribute();
            var context = MakeContext(jwt);

            filter.OnActionExecuting(context);

            var result = Assert.IsType<ObjectResult>(context.Result);
            Assert.Equal(403, result.StatusCode);
        }

        [Fact]
        public void OnActionExecuting_DoesNotSetResult_WhenIsAdminTrue()
        {
            var jwt = BuildJwt("{\"sub\":\"1\",\"isAdmin\":\"true\"}");
            var filter = new AppAdminAttribute();
            var context = MakeContext(jwt);

            filter.OnActionExecuting(context);

            Assert.Null(context.Result);
        }

        [Fact]
        public void OnActionExecuting_Returns_FORBIDDEN_MessageCode()
        {
            var filter = new AppAdminAttribute();
            var context = MakeContext(null);

            filter.OnActionExecuting(context);

            var result = Assert.IsType<ObjectResult>(context.Result);
            var error = Assert.IsType<ErrorResponse>(result.Value);
            Assert.Equal("FORBIDDEN", error.Message);
        }
    }
}
