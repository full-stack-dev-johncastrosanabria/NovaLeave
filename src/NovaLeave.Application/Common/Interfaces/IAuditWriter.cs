namespace NovaLeave.Application.Common.Interfaces;

public interface IAuditWriter
{
    Task WriteAsync(
        string actorId,
        string actorRole,
        string action,
        string entityType,
        Guid entityId,
        string result,
        Guid correlationId,
        Guid requestId,
        string? data,
        CancellationToken cancellationToken = default);
}
