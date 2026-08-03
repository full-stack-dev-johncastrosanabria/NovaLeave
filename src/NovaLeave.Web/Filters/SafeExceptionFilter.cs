using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;

namespace NovaLeave.Web.Filters;

public sealed class SafeExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var environment = context.HttpContext.RequestServices.GetService<IHostEnvironment>();
        if (environment?.IsEnvironment("Testing") == true)
        {
            context.ExceptionHandled = false;
            return;
        }

        var correlationId = context.HttpContext.Items.TryGetValue("correlation_id", out var value)
            ? value?.ToString()
            : context.HttpContext.TraceIdentifier;
        var safePayload = new { message = "La operación no pudo completarse.", correlation_id = correlationId };

        context.Result = context.Exception switch
        {
            UnauthorizedAccessException => new ForbidResult(),
            KeyNotFoundException => new NotFoundResult(),
            InvalidOperationException => new ConflictObjectResult(safePayload),
            _ => new ObjectResult(new { message = "Ocurrió un error inesperado.", correlation_id = correlationId }) { StatusCode = StatusCodes.Status500InternalServerError }
        };

        context.ExceptionHandled = true;
    }
}
