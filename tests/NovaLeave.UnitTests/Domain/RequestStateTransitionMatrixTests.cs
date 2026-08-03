using NovaLeave.Domain.Entities;
using NovaLeave.Domain.Enums;
using NovaLeave.Domain.ValueObjects;

namespace NovaLeave.UnitTests.Domain;

public sealed class RequestStateTransitionMatrixTests
{
    [Theory]
    [InlineData(RequestStatus.Pending, "Approve", true)]
    [InlineData(RequestStatus.Pending, "Reject", true)]
    [InlineData(RequestStatus.Pending, "Timeout", true)]
    [InlineData(RequestStatus.Pending, "Deactivate", false)]
    [InlineData(RequestStatus.Approved, "Approve", false)]
    [InlineData(RequestStatus.Approved, "Reject", false)]
    [InlineData(RequestStatus.Approved, "Timeout", false)]
    [InlineData(RequestStatus.Approved, "Deactivate", true)]
    [InlineData(RequestStatus.Rejected, "Approve", false)]
    [InlineData(RequestStatus.Rejected, "Reject", false)]
    [InlineData(RequestStatus.Rejected, "Timeout", false)]
    [InlineData(RequestStatus.Rejected, "Deactivate", false)]
    [InlineData(RequestStatus.CancelledByTimeout, "Approve", false)]
    [InlineData(RequestStatus.CancelledByTimeout, "Reject", false)]
    [InlineData(RequestStatus.CancelledByTimeout, "Timeout", false)]
    [InlineData(RequestStatus.CancelledByTimeout, "Deactivate", false)]
    [InlineData(RequestStatus.CancelledByApprover, "Approve", false)]
    [InlineData(RequestStatus.CancelledByApprover, "Reject", false)]
    [InlineData(RequestStatus.CancelledByApprover, "Timeout", false)]
    [InlineData(RequestStatus.CancelledByApprover, "Deactivate", false)]
    public void Official_Lifecycle_Matrix_Allows_Only_Approved_Transitions(RequestStatus from, string transition, bool allowed)
    {
        var request = InState(from);

        var exception = Record.Exception(() => Apply(request, transition));

        if (allowed)
        {
            Assert.Null(exception);
        }
        else
        {
            Assert.IsType<InvalidOperationException>(exception);
        }
    }

    [Fact]
    public void Approved_Request_Cannot_Be_Deactivated_On_Or_After_Start_Date()
    {
        var request = InState(RequestStatus.Approved);

        Assert.Throws<InvalidOperationException>(() =>
            request.CancelByApprover("approver-2", new DateOnly(2027, 1, 4), DateTimeOffset.UtcNow));
    }

    private static VacationRequest InState(RequestStatus status)
    {
        var request = VacationRequest.Create(
            "owner-1",
            new DateRange(new DateOnly(2027, 1, 4), new DateOnly(2027, 1, 8)),
            WorkingDayCount.From(5),
            "Vacaciones familiares planificadas.",
            DateTimeOffset.UtcNow);

        switch (status)
        {
            case RequestStatus.Pending:
                return request;
            case RequestStatus.Approved:
                request.Approve("approver-1", DateTimeOffset.UtcNow);
                return request;
            case RequestStatus.Rejected:
                request.Reject("approver-1", "Solicitud rechazada por regla valida.", DateTimeOffset.UtcNow);
                return request;
            case RequestStatus.CancelledByTimeout:
                request.CancelByTimeout(DateTimeOffset.UtcNow);
                return request;
            case RequestStatus.CancelledByApprover:
                request.Approve("approver-1", DateTimeOffset.UtcNow);
                request.CancelByApprover("approver-2", new DateOnly(2027, 1, 3), DateTimeOffset.UtcNow);
                return request;
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }
    }

    private static void Apply(VacationRequest request, string transition)
    {
        switch (transition)
        {
            case "Approve":
                request.Approve("approver-1", DateTimeOffset.UtcNow);
                break;
            case "Reject":
                request.Reject("approver-1", "Solicitud rechazada por regla valida.", DateTimeOffset.UtcNow);
                break;
            case "Timeout":
                request.CancelByTimeout(DateTimeOffset.UtcNow);
                break;
            case "Deactivate":
                request.CancelByApprover("approver-2", new DateOnly(2027, 1, 3), DateTimeOffset.UtcNow);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(transition), transition, null);
        }
    }
}
