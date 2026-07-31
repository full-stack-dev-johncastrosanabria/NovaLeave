using Microsoft.AspNetCore.Mvc.Filters;

namespace NovaLeave.Web.Filters;

public sealed class OverpostingPreventionFilter : IActionFilter
{
    private static readonly HashSet<string> ProhibitedFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "OwnerId",
        "Status",
        "Balance",
        "AvailableDays",
        "WorkingDays",
        "LeaveType"
    };

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.HttpContext.Request.HasFormContentType)
        {
            return;
        }

        var form = context.HttpContext.Request.Form;
        if (form.Keys.Any(key => ProhibitedFields.Contains(key)))
        {
            context.ModelState.AddModelError(string.Empty, "La solicitud contiene campos no permitidos.");
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
