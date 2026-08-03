using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Hosting;
using NovaLeave.Web.ViewModels;

namespace NovaLeave.Web.Filters;

public sealed class SafeExceptionFilter : IExceptionFilter
{
    private readonly ILogger<SafeExceptionFilter> _logger;

    public SafeExceptionFilter(ILogger<SafeExceptionFilter> logger)
    {
        _logger = logger;
    }

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
        if (context.Exception is UnauthorizedAccessException)
        {
            context.Result = new ForbidResult();
        }
        else if (context.Exception is KeyNotFoundException)
        {
            context.Result = new NotFoundResult();
        }
        else
        {
            _logger.LogError(context.Exception, "Unhandled request failure. CorrelationId: {CorrelationId}", correlationId);
            var model = new UnexpectedErrorViewModel(correlationId ?? context.HttpContext.TraceIdentifier);
            context.Result = new ViewResult
            {
                ViewName = "Error",
                StatusCode = StatusCodes.Status500InternalServerError,
                ViewData = new ViewDataDictionary<UnexpectedErrorViewModel>(
                    new EmptyModelMetadataProvider(),
                    context.ModelState)
                {
                    Model = model
                }
            };
        }

        context.ExceptionHandled = true;
    }
}
