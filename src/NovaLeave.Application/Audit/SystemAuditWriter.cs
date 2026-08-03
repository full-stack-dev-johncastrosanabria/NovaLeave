using NovaLeave.Domain.Entities;

namespace NovaLeave.Application.Audit;

public sealed class SystemAuditWriter
{
    public const string ActorId = "System";
    public const string ActorRole = "System";

    public AuditRecord Timeout(Guid requestId, int timeoutDays, DateTimeOffset timestampUtc)
    {
        return AuditRecord.Create(
            ActorId,
            ActorRole,
            "Timeout",
            nameof(VacationRequest),
            requestId,
            "Success",
            Guid.NewGuid(),
            requestId,
            $"{{\"Rule\":\"PendingRequestTimeout\",\"PendingRequestTimeoutDays\":{timeoutDays}}}",
            timestampUtc);
    }

    public AuditRecord Accrual(Guid balanceId, string userId, DateOnly accrualPeriod, DateTimeOffset timestampUtc)
    {
        return AuditRecord.Create(
            ActorId,
            ActorRole,
            "Accrual",
            "VacationBalance",
            balanceId,
            "Success",
            Guid.NewGuid(),
            Guid.NewGuid(),
            $"{{\"Rule\":\"MonthlyAccrual\",\"UserId\":\"{userId}\",\"AccrualPeriod\":\"{accrualPeriod:yyyy-MM}\"}}",
            timestampUtc);
    }
}
