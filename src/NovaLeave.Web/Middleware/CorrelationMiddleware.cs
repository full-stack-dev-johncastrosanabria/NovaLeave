using System.Diagnostics;
using NovaLeave.Application.Observability;

namespace NovaLeave.Web.Middleware;

public sealed class CorrelationMiddleware
{
    public const string CorrelationHeader = "X-Correlation-ID";

    private readonly RequestDelegate _next;
    private readonly IOperationalTelemetry _telemetry;

    public CorrelationMiddleware(RequestDelegate next, IOperationalTelemetry telemetry)
    {
        _next = next;
        _telemetry = telemetry;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ReadOrCreateCorrelationId(context);
        var requestId = context.TraceIdentifier;
        context.Items["correlation_id"] = correlationId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationHeader] = correlationId;
            return Task.CompletedTask;
        });

        using var logScope = _telemetry.BeginLogScope(correlationId, requestId);
        using var activity = _telemetry.StartOperation("http.request", correlationId);
        activity?.SetTag("request_id", requestId);
        activity?.SetTag("http.method", context.Request.Method);
        activity?.SetTag("url.path", context.Request.Path.Value);
        var started = Stopwatch.GetTimestamp();

        try
        {
            await _next(context);
        }
        finally
        {
            var elapsed = Stopwatch.GetElapsedTime(started);
            _telemetry.RecordHttpRequest(
                context.Request.Method,
                context.Request.Path.Value ?? "/",
                context.Response.StatusCode,
                elapsed);
            activity?.SetTag("http.status_code", context.Response.StatusCode);
        }
    }

    private static string ReadOrCreateCorrelationId(HttpContext context)
    {
        var candidate = context.Request.Headers[CorrelationHeader].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(candidate) && candidate.Length <= 128)
        {
            return candidate.Trim();
        }

        return Guid.NewGuid().ToString("N");
    }
}
