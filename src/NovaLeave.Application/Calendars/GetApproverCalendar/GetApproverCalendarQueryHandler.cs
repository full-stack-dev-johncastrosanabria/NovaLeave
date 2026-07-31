using NovaLeave.Application.Authorization;
using NovaLeave.Application.Common.Interfaces;
using NovaLeave.Application.Common.Results;
using NovaLeave.Domain.Enums;

namespace NovaLeave.Application.Calendars.GetApproverCalendar;

public sealed record ApproverCalendarEvent(Guid RequestId, DateOnly StartDate, DateOnly EndDate, int WorkingDays);

public sealed class GetApproverCalendarQueryHandler
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IApproverIdentityService _identityService;

    public GetApproverCalendarQueryHandler(IApplicationDbContext dbContext, IApproverIdentityService identityService)
    {
        _dbContext = dbContext;
        _identityService = identityService;
    }

    public async Task<Result<IReadOnlyList<ApproverCalendarEvent>>> HandleAsync(string approverId, CancellationToken cancellationToken)
    {
        var approver = await _identityService.GetApproverAsync(approverId, cancellationToken);
        if (ApproverPolicies.CanViewQueue(approver) is { } authorization)
        {
            return Result<IReadOnlyList<ApproverCalendarEvent>>.Failure(authorization);
        }

        var events = _dbContext.VacationRequests
            .Where(request => request.Status == RequestStatus.Approved)
            .OrderBy(request => request.StartDate)
            .Select(request => new ApproverCalendarEvent(request.Id, request.StartDate, request.EndDate, request.WorkingDays))
            .ToList();

        return Result<IReadOnlyList<ApproverCalendarEvent>>.Success(events);
    }
}
