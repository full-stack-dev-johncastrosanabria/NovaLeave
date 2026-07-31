using NovaLeave.Application.Requests.Models;

namespace NovaLeave.Application.Requests.CreateVacationRequest;

public sealed record CreateVacationRequestCommand(
    string UserId,
    RequestInputMode InputMode,
    DateOnly StartDate,
    DateOnly? EndDate,
    int? WorkingDays,
    string Reason);
