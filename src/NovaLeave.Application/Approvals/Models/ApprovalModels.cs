using NovaLeave.Domain.Enums;

namespace NovaLeave.Application.Approvals.Models;

public sealed record ApproverRequestSummary(
    Guid Id,
    string RequesterId,
    DateOnly StartDate,
    DateOnly EndDate,
    int WorkingDays,
    int AvailableDays,
    int ProjectedBalanceAfterApproval);

public sealed record ApproverRequestDetail(
    Guid Id,
    string RequesterId,
    string RequesterName,
    DateOnly StartDate,
    DateOnly EndDate,
    int WorkingDays,
    RequestStatus Status,
    int AvailableDays,
    int ProjectedBalanceAfterApproval,
    bool CanApproveOrReject,
    bool CanDeactivate,
    bool HasOverlapWarning,
    byte[] RowVersion);

public sealed record ResolutionHistoryItem(DateTime TimestampUtc, string Action, Guid RequestId, string RequesterId, RequestStatus Status);
