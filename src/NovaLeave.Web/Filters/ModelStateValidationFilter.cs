using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NovaLeave.Web.Filters;

public sealed class ModelStateValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (string.Equals(context.RouteData.Values["area"]?.ToString(), "Identity", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (!context.ModelState.IsValid && context.HttpContext.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new BadRequestObjectResult(context.ModelState);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
