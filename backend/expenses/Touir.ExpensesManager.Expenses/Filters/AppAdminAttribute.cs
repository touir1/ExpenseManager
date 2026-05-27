using Touir.ExpensesManager.Expenses.Controllers.Responses;
using Touir.ExpensesManager.Expenses.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Touir.ExpensesManager.Expenses.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AppAdminAttribute : Attribute, IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!JwtCookieReader.GetIsAdmin(context.HttpContext.Request))
                context.Result = new ObjectResult(new ErrorResponse { Message = "FORBIDDEN" }) { StatusCode = 403 };
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
