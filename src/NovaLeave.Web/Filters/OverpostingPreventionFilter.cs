using Microsoft.AspNetCore.Mvc.Filters;

namespace NovaLeave.Web.Filters;

public sealed class OverpostingPreventionFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _ = context;
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
