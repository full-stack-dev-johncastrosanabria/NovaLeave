using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NovaLeave.Web.Filters;

public sealed class ModelStateValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid || !context.HttpContext.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // Actions that render a form redisplay their own view with the errors bound to the
        // offending fields, so the person sees the message next to the input rather than a
        // machine-readable payload (constitution §11.3 and §11.4).
        var redisplaysForm = context.ActionDescriptor.EndpointMetadata
            .OfType<RedisplayFormOnInvalidModelAttribute>()
            .Any();

        if (!redisplaysForm)
        {
            context.Result = new BadRequestObjectResult(context.ModelState);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
