namespace NovaLeave.Domain.Enums;

public enum RequestStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    CancelledByTimeout = 3,
    CancelledByApprover = 4
}
