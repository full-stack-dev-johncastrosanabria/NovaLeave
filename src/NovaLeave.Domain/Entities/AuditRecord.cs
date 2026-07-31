namespace NovaLeave.Domain.Entities;

public sealed class AuditRecord
{
    private AuditRecord()
    {
        ActorId = string.Empty;
        ActorRole = string.Empty;
        Action = string.Empty;
        EntityType = string.Empty;
        Result = string.Empty;
    }

    private AuditRecord(
        string actorId,
        string actorRole,
        string action,
        string entityType,
        Guid entityId,
        string result,
        Guid correlationId,
        Guid requestId,
        string? data,
        DateTimeOffset timestampUtc)
    {
        RejectSensitiveData(data);
        Id = Guid.NewGuid();
        ActorId = RequireText(actorId, nameof(actorId));
        ActorRole = RequireText(actorRole, nameof(actorRole));
        Action = RequireText(action, nameof(action));
        EntityType = RequireText(entityType, nameof(entityType));
        EntityId = entityId;
        Result = RequireText(result, nameof(result));
        CorrelationId = correlationId;
        RequestId = requestId;
        Data = data;
        TimestampUtc = timestampUtc.UtcDateTime;
    }

    public Guid Id { get; private set; }

    public DateTime TimestampUtc { get; private set; }

    public string ActorId { get; private set; }

    public string ActorRole { get; private set; }

    public string Action { get; private set; }

    public string EntityType { get; private set; }

    public Guid EntityId { get; private set; }

    public string Result { get; private set; }

    public Guid CorrelationId { get; private set; }

    public Guid RequestId { get; private set; }

    public string? Data { get; private set; }

    public static AuditRecord Create(
        string actorId,
        string actorRole,
        string action,
        string entityType,
        Guid entityId,
        string result,
        Guid correlationId,
        Guid requestId,
        string? data,
        DateTimeOffset timestampUtc)
    {
        return new AuditRecord(actorId, actorRole, action, entityType, entityId, result, correlationId, requestId, data, timestampUtc);
    }

    private static void RejectSensitiveData(string? data)
    {
        if (data is null)
        {
            return;
        }

        if (data.Contains("\"Reason\"", StringComparison.OrdinalIgnoreCase) ||
            data.Contains("\"RejectionReason\"", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Audit data must not contain sensitive reason fields.");
        }
    }

    private static string RequireText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", parameterName);
        }

        return value.Trim();
    }
}
