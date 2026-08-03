using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NovaLeave.Application.Observability;
using Serilog.Context;

namespace NovaLeave.Infrastructure.Observability;

public sealed class OperationalTelemetry : IOperationalTelemetry
{
    private static readonly Meter Meter = new(NovaLeaveTelemetry.MeterName, "1.0.0");
    private static readonly ActivitySource ActivitySource = new(NovaLeaveTelemetry.ActivitySourceName);
    private static readonly Counter<long> HttpRequests = Meter.CreateCounter<long>("novaleave.http.requests");
    private static readonly Histogram<double> HttpRequestDuration = Meter.CreateHistogram<double>("novaleave.http.request.duration", "ms");
    private static readonly Counter<long> HttpErrors = Meter.CreateCounter<long>("novaleave.http.errors");
    private static readonly Counter<long> RequestsCreated = Meter.CreateCounter<long>("novaleave.vacation_request.created");
    private static readonly Counter<long> RequestsApproved = Meter.CreateCounter<long>("novaleave.vacation_request.approved");
    private static readonly Counter<long> RequestsRejected = Meter.CreateCounter<long>("novaleave.vacation_request.rejected");
    private static readonly Counter<long> TimeoutCancelled = Meter.CreateCounter<long>("novaleave.job.timeout_cancelled");
    private static readonly Counter<long> AccrualExecuted = Meter.CreateCounter<long>("novaleave.job.accrual_executed");
    private static readonly Counter<long> ConcurrencyConflicts = Meter.CreateCounter<long>("novaleave.concurrency.conflicts");
    private static readonly Counter<long> InvariantViolations = Meter.CreateCounter<long>("novaleave.invariant.violations");
    private static readonly Counter<long> AlertableEvents = Meter.CreateCounter<long>("novaleave.alertable_events");

    private readonly ILogger<OperationalTelemetry> _logger;

    public OperationalTelemetry(ILogger<OperationalTelemetry> logger)
    {
        _logger = logger;
    }

    public IDisposable? BeginLogScope(string correlationId, string requestId)
    {
        return new LogScope(
            LogContext.PushProperty("correlation_id", correlationId),
            LogContext.PushProperty("request_id", requestId));
    }

    public Activity? StartOperation(string operationName, string correlationId, string? data = null)
    {
        var activity = ActivitySource.StartActivity(operationName);
        if (activity is null)
        {
            return null;
        }

        activity.SetTag("correlation_id", correlationId);
        if (!string.IsNullOrWhiteSpace(data))
        {
            activity.SetTag("data", ObservabilityRedactor.Redact(data));
        }

        return activity;
    }

    public void RecordHttpRequest(string method, string path, int statusCode, TimeSpan duration)
    {
        var tags = new TagList
        {
            { "method", method },
            { "path", path },
            { "status_code", statusCode }
        };
        HttpRequests.Add(1, tags);
        HttpRequestDuration.Record(duration.TotalMilliseconds, tags);
        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            HttpErrors.Add(1, tags);
            RecordAlertableEvent("http.5xx");
        }
    }

    public void RecordRequestCreated(bool success)
    {
        RequestsCreated.Add(1, Outcome(success));
    }

    public void RecordApproval(bool success)
    {
        RequestsApproved.Add(1, Outcome(success));
    }

    public void RecordRejection(bool success)
    {
        RequestsRejected.Add(1, Outcome(success));
    }

    public void RecordTimeoutCancellation(bool success)
    {
        TimeoutCancelled.Add(1, Outcome(success));
        if (!success)
        {
            RecordAlertableEvent("job.timeout.failure");
        }
    }

    public void RecordAccrualExecution(bool success)
    {
        AccrualExecuted.Add(1, Outcome(success));
        if (!success)
        {
            RecordAlertableEvent("job.accrual.failure");
        }
    }

    public void RecordConcurrencyConflict(string operation)
    {
        ConcurrencyConflicts.Add(1, new TagList { { "operation", ObservabilityRedactor.SafeLabel(operation) } });
    }

    public void RecordInvariantViolation(string invariant)
    {
        var safeInvariant = ObservabilityRedactor.SafeLabel(invariant);
        InvariantViolations.Add(1, new TagList { { "invariant", safeInvariant } });
        RecordAlertableEvent("invariant.failure");
        _logger.LogWarning("Business invariant violation detected: {Invariant}", safeInvariant);
    }

    public void RecordAlertableEvent(string condition)
    {
        AlertableEvents.Add(1, new TagList { { "condition", ObservabilityRedactor.SafeLabel(condition) } });
        _logger.LogWarning("Alertable operational condition detected: {Condition}", ObservabilityRedactor.SafeLabel(condition));
    }

    private static TagList Outcome(bool success)
    {
        return new TagList { { "result", success ? "success" : "failure" } };
    }

    private sealed class LogScope : IDisposable
    {
        private readonly IDisposable _correlationScope;
        private readonly IDisposable _requestScope;

        public LogScope(IDisposable correlationScope, IDisposable requestScope)
        {
            _correlationScope = correlationScope;
            _requestScope = requestScope;
        }

        public void Dispose()
        {
            _requestScope.Dispose();
            _correlationScope.Dispose();
        }
    }
}
