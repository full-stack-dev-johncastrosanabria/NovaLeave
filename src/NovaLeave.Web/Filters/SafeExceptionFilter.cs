using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NovaLeave.Web.Filters;

public sealed class SafeExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        context.Result = context.Exception switch
        {
            UnauthorizedAccessException => new ForbidResult(),
            KeyNotFoundException => new NotFoundResult(),
            InvalidOperationException => new ConflictObjectResult("La operación no pudo completarse."),
            _ => new ObjectResult("Ocurrió un error inesperado.") { StatusCode = StatusCodes.Status500InternalServerError }
        };

        context.ExceptionHandled = true;
    }
}
