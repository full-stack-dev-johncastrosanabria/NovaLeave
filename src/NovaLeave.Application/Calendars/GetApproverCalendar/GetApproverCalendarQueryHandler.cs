using NovaLeave.Application.Authorization;
using NovaLeave.Application.Common.Interfaces;
using NovaLeave.Application.Common.Results;
using NovaLeave.Domain.Enums;

namespace NovaLeave.Application.Calendars.GetApproverCalendar;

public sealed record ApproverCalendarEvent(
    Guid RequestId,
    DateOnly StartDate,
    DateOnly EndDate,
    int WorkingDays,
    RequestStatus Status,
    CalendarEventCategory Category,
    bool CanNavigateToDetail);

public sealed class GetApproverCalendarQueryHandler
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IApproverIdentityService _identityService;

    public GetApproverCalendarQueryHandler(IApplicationDbContext dbContext, IApproverIdentityService identityService)
    {
        _dbContext = dbContext;
        _identityService = identityService;
    }

    public async Task<Result<IReadOnlyList<ApproverCalendarEvent>>> HandleAsync(string approverId, bool includeOwnRequests, CancellationToken cancellationToken)
    {
        var approver = await _identityService.GetApproverAsync(approverId, cancellationToken);
        if (ApproverPolicies.CanViewQueue(approver) is { } authorization)
        {
            return Result<IReadOnlyList<ApproverCalendarEvent>>.Failure(authorization);
        }

        IEnumerable<ApproverCalendarEvent> ownRequests = includeOwnRequests
            ? _dbContext.VacationRequests
                .Where(request => request.OwnerId == approverId && (request.Status == RequestStatus.Pending || request.Status == RequestStatus.Approved))
                .Select(request => new ApproverCalendarEvent(
                    request.Id,
                    request.StartDate,
                    request.EndDate,
                    request.WorkingDays,
                    request.Status,
                    CalendarEventCategory.OwnRequest,
                    true))
            : [];

        var pendingApprovals = _dbContext.VacationRequests
            .Where(request => request.Status == RequestStatus.Pending && request.OwnerId != approverId)
            .Select(request => new ApproverCalendarEvent(
                request.Id,
                request.StartDate,
                request.EndDate,
                request.WorkingDays,
                request.Status,
                CalendarEventCategory.PendingApproval,
                true));

        var approvedOrganization = _dbContext.VacationRequests
            .Where(request => request.Status == RequestStatus.Approved && request.OwnerId != approverId)
            .Select(request => new ApproverCalendarEvent(
                request.Id,
                request.StartDate,
                request.EndDate,
                request.WorkingDays,
                request.Status,
                CalendarEventCategory.ApprovedOrganization,
                false));

        var resolvedByMe = _dbContext.AuditRecords
            .Where(audit => audit.ActorId == approverId && (audit.Action == "Approve" || audit.Action == "Reject" || audit.Action == "Deactivate"))
            .AsEnumerable()
            .Select(audit => _dbContext.VacationRequests.SingleOrDefault(request => request.Id == audit.EntityId))
            .Where(request => request is not null && request.OwnerId != approverId)
            .Select(request => new ApproverCalendarEvent(
                request!.Id,
                request.StartDate,
                request.EndDate,
                request.WorkingDays,
                request.Status,
                CalendarEventCategory.ResolvedByMe,
                false));

        var events = ownRequests
            .Concat(pendingApprovals)
            .Concat(approvedOrganization)
            .Concat(resolvedByMe)
            .OrderBy(calendarEvent => calendarEvent.StartDate)
            .ThenBy(calendarEvent => calendarEvent.EndDate)
            .ThenBy(calendarEvent => calendarEvent.Category)
            .ToList();

        return Result<IReadOnlyList<ApproverCalendarEvent>>.Success(events);
    }
}
