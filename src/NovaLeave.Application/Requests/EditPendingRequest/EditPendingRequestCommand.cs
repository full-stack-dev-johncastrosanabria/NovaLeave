using NovaLeave.Application.Requests.Models;

namespace NovaLeave.Application.Requests.EditPendingRequest;

public sealed record EditPendingRequestCommand(
    string UserId,
    Guid RequestId,
    RequestInputMode InputMode,
    DateOnly StartDate,
    DateOnly? EndDate,
    int? WorkingDays,
    string Reason,
    byte[] RowVersion);
