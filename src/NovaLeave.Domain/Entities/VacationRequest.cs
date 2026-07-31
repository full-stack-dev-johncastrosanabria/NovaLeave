using NovaLeave.Domain.Enums;
using NovaLeave.Domain.ValueObjects;

namespace NovaLeave.Domain.Entities;

public sealed class VacationRequest
{
    private VacationRequest()
    {
        OwnerId = string.Empty;
        Reason = string.Empty;
        RowVersion = [];
    }

    private VacationRequest(
        Guid id,
        string ownerId,
        DateRange dateRange,
        WorkingDayCount workingDays,
        string reason,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        OwnerId = RequireText(ownerId, nameof(ownerId));
        StartDate = dateRange.Start;
        EndDate = dateRange.End;
        WorkingDays = workingDays.Value;
        Reason = RequireText(reason, nameof(reason));
        Status = RequestStatus.Pending;
        LeaveType = LeaveType.Vacation;
        CreatedAtUtc = createdAtUtc.UtcDateTime;
        UpdatedAtUtc = createdAtUtc.UtcDateTime;
        RowVersion = [];
    }

    public Guid Id { get; private set; }

    public string OwnerId { get; private set; }

    public LeaveType LeaveType { get; private set; }

    public DateOnly StartDate { get; private set; }

    public DateOnly EndDate { get; private set; }

    public int WorkingDays { get; private set; }

    public string Reason { get; private set; }

    public RequestStatus Status { get; private set; }

    public string? RejectionReason { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public byte[] RowVersion { get; private set; }

    public DateRange DateRange => new(StartDate, EndDate);

    public static VacationRequest Create(
        string ownerId,
        DateRange dateRange,
        WorkingDayCount workingDays,
        string reason,
        DateTimeOffset createdAtUtc)
    {
        return new VacationRequest(Guid.NewGuid(), ownerId, dateRange, workingDays, reason, createdAtUtc);
    }

    public void Approve(string approverId, DateTimeOffset timestampUtc)
    {
        _ = RequireText(approverId, nameof(approverId));
        EnsureStatus(RequestStatus.Pending);
        Status = RequestStatus.Approved;
        Touch(timestampUtc);
    }

    public void Reject(string approverId, string rejectionReason, DateTimeOffset timestampUtc)
    {
        _ = RequireText(approverId, nameof(approverId));
        EnsureStatus(RequestStatus.Pending);
        RejectionReason = RequireText(rejectionReason, nameof(rejectionReason));
        Status = RequestStatus.Rejected;
        Touch(timestampUtc);
    }

    public void EditPending(DateRange dateRange, WorkingDayCount workingDays, string reason, DateTimeOffset timestampUtc)
    {
        EnsureStatus(RequestStatus.Pending);
        StartDate = dateRange.Start;
        EndDate = dateRange.End;
        WorkingDays = workingDays.Value;
        Reason = RequireText(reason, nameof(reason));
        Touch(timestampUtc);
    }

    public void CancelByTimeout(DateTimeOffset timestampUtc)
    {
        EnsureStatus(RequestStatus.Pending);
        Status = RequestStatus.CancelledByTimeout;
        Touch(timestampUtc);
    }

    public void CancelByApprover(string approverId, DateOnly businessDate, DateTimeOffset timestampUtc)
    {
        _ = RequireText(approverId, nameof(approverId));
        EnsureStatus(RequestStatus.Approved);

        if (StartDate <= businessDate)
        {
            throw new InvalidOperationException("Approved request can only be cancelled before its start date.");
        }

        Status = RequestStatus.CancelledByApprover;
        Touch(timestampUtc);
    }

    private static string RequireText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", parameterName);
        }

        return value.Trim();
    }

    private void EnsureStatus(RequestStatus expected)
    {
        if (Status != expected)
        {
            throw new InvalidOperationException($"Request status must be {expected}.");
        }
    }

    private void Touch(DateTimeOffset timestampUtc)
    {
        UpdatedAtUtc = timestampUtc.UtcDateTime;
    }
}
