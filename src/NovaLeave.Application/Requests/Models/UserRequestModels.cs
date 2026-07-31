using NovaLeave.Domain.Enums;

namespace NovaLeave.Application.Requests.Models;

public sealed record UserRequestSummary(
    Guid Id,
    DateOnly StartDate,
    DateOnly EndDate,
    int WorkingDays,
    RequestStatus Status,
    DateTime CreatedAtUtc);

public sealed record UserRequestDetail(
    Guid Id,
    DateOnly StartDate,
    DateOnly EndDate,
    int WorkingDays,
    RequestStatus Status,
    string Reason,
    string? RejectionReason,
    byte[] RowVersion,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public enum RequestInputMode
{
    DateRange = 1,
    StartPlusDays = 2
}
