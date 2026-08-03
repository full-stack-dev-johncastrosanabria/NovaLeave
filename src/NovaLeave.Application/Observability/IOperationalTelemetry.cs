using System.Diagnostics;

namespace NovaLeave.Application.Observability;

public interface IOperationalTelemetry
{
    IDisposable? BeginLogScope(string correlationId, string requestId);

    Activity? StartOperation(string operationName, string correlationId, string? data = null);

    void RecordHttpRequest(string method, string path, int statusCode, TimeSpan duration);

    void RecordRequestCreated(bool success);

    void RecordApproval(bool success);

    void RecordRejection(bool success);

    void RecordTimeoutCancellation(bool success);

    void RecordAccrualExecution(bool success);

    void RecordConcurrencyConflict(string operation);

    void RecordInvariantViolation(string invariant);

    void RecordAlertableEvent(string condition);
}
