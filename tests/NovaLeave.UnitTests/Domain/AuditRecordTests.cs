using NovaLeave.Domain.Entities;

namespace NovaLeave.UnitTests.Domain;

public sealed class AuditRecordTests
{
    [Fact]
    public void Audit_Record_Requires_Constitutional_Fields()
    {
        var record = AuditRecord.Create(
            actorId: "actor-1",
            actorRole: "Approver",
            action: "Approve",
            entityType: "VacationRequest",
            entityId: Guid.NewGuid(),
            result: "Success",
            correlationId: Guid.NewGuid(),
            requestId: Guid.NewGuid(),
            data: "{\"Status\":\"Approved\"}",
            timestampUtc: DateTimeOffset.UtcNow);

        Assert.NotEqual(Guid.Empty, record.Id);
        Assert.NotEqual(default, record.TimestampUtc);
        Assert.Equal("actor-1", record.ActorId);
        Assert.Equal("Approver", record.ActorRole);
        Assert.Equal("Approve", record.Action);
        Assert.Equal("VacationRequest", record.EntityType);
        Assert.Equal("Success", record.Result);
        Assert.NotEqual(Guid.Empty, record.CorrelationId);
        Assert.NotEqual(Guid.Empty, record.RequestId);
    }

    [Theory]
    [InlineData("{\"Reason\":\"medical detail\"}")]
    [InlineData("{\"RejectionReason\":\"private detail\"}")]
    [InlineData("{\"reason\":\"private detail\"}")]
    public void Audit_Record_Data_Rejects_Sensitive_Reason_Fields(string data)
    {
        Assert.Throws<InvalidOperationException>(() => AuditRecord.Create(
            actorId: "actor-1",
            actorRole: "Approver",
            action: "Reject",
            entityType: "VacationRequest",
            entityId: Guid.NewGuid(),
            result: "Success",
            correlationId: Guid.NewGuid(),
            requestId: Guid.NewGuid(),
            data: data,
            timestampUtc: DateTimeOffset.UtcNow));
    }
}
