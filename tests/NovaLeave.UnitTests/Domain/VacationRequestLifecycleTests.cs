using NovaLeave.Domain.Entities;
using NovaLeave.Domain.Enums;
using NovaLeave.Domain.ValueObjects;

namespace NovaLeave.UnitTests.Domain;

public sealed class VacationRequestLifecycleTests
{
    [Fact]
    public void RequestStatus_Defines_Exactly_Five_Approved_States()
    {
        var states = Enum.GetNames<RequestStatus>();

        Assert.Equal(
            ["Pending", "Approved", "Rejected", "CancelledByTimeout", "CancelledByApprover"],
            states);
    }

    [Fact]
    public void Pending_Request_Can_Only_Approve_Reject_Or_Timeout()
    {
        var request = CreatePendingRequest();

        request.Approve("approver-1", DateTimeOffset.UtcNow);
        Assert.Equal(RequestStatus.Approved, request.Status);

        Assert.Throws<InvalidOperationException>(() => request.Reject("approver-1", "reason", DateTimeOffset.UtcNow));
        Assert.Throws<InvalidOperationException>(() => request.CancelByTimeout(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Pending_Request_Can_Be_Rejected()
    {
        var request = CreatePendingRequest();

        request.Reject("approver-1", "Solicitud rechazada por regla valida.", DateTimeOffset.UtcNow);

        Assert.Equal(RequestStatus.Rejected, request.Status);
    }

    [Fact]
    public void Pending_Request_Can_Be_Cancelled_By_Timeout()
    {
        var request = CreatePendingRequest();

        request.CancelByTimeout(DateTimeOffset.UtcNow);

        Assert.Equal(RequestStatus.CancelledByTimeout, request.Status);
    }

    [Fact]
    public void Approved_Request_Can_Only_Be_Cancelled_By_Approver_Before_Start()
    {
        var request = CreatePendingRequest();
        request.Approve("approver-1", DateTimeOffset.UtcNow);

        request.CancelByApprover("approver-2", new DateOnly(2027, 1, 3), DateTimeOffset.UtcNow);

        Assert.Equal(RequestStatus.CancelledByApprover, request.Status);
    }

    [Theory]
    [InlineData(RequestStatus.Rejected)]
    [InlineData(RequestStatus.CancelledByTimeout)]
    [InlineData(RequestStatus.CancelledByApprover)]
    public void Terminal_States_Reject_Further_Transitions(RequestStatus terminalStatus)
    {
        var request = CreatePendingRequest();
        MoveTo(request, terminalStatus);

        Assert.Throws<InvalidOperationException>(() => request.Approve("approver-1", DateTimeOffset.UtcNow));
        Assert.Throws<InvalidOperationException>(() => request.Reject("approver-1", "reason", DateTimeOffset.UtcNow));
        Assert.Throws<InvalidOperationException>(() => request.CancelByTimeout(DateTimeOffset.UtcNow));
        Assert.Throws<InvalidOperationException>(() => request.CancelByApprover("approver-1", new DateOnly(2027, 1, 9), DateTimeOffset.UtcNow));
    }

    private static VacationRequest CreatePendingRequest()
    {
        return VacationRequest.Create(
            "owner-1",
            new DateRange(new DateOnly(2027, 1, 4), new DateOnly(2027, 1, 8)),
            WorkingDayCount.From(5),
            "Vacaciones familiares planificadas.",
            DateTimeOffset.UtcNow);
    }

    private static void MoveTo(VacationRequest request, RequestStatus status)
    {
        switch (status)
        {
            case RequestStatus.Rejected:
                request.Reject("approver-1", "Solicitud rechazada por regla valida.", DateTimeOffset.UtcNow);
                break;
            case RequestStatus.CancelledByTimeout:
                request.CancelByTimeout(DateTimeOffset.UtcNow);
                break;
            case RequestStatus.CancelledByApprover:
                request.Approve("approver-1", DateTimeOffset.UtcNow);
                request.CancelByApprover("approver-2", new DateOnly(2027, 1, 3), DateTimeOffset.UtcNow);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }
    }
}
