using NovaLeave.Application.Approvals.Models;
using NovaLeave.Application.Authorization;
using NovaLeave.Application.Common.Errors;
using NovaLeave.Application.Common.Interfaces;
using NovaLeave.Application.Common.Results;
using NovaLeave.Domain.Enums;
using NovaLeave.Domain.Services;

namespace NovaLeave.Application.Approvals.Queries;

public sealed class GetApproverRequestDetailQueryHandler
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IApproverIdentityService _identityService;
    private readonly IUserDirectory _userDirectory;
    private readonly OverlapPolicy _overlapPolicy;
    private readonly TimeProvider _timeProvider;

    public GetApproverRequestDetailQueryHandler(IApplicationDbContext dbContext, IApproverIdentityService identityService, IUserDirectory userDirectory, OverlapPolicy overlapPolicy, TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _identityService = identityService;
        _userDirectory = userDirectory;
        _overlapPolicy = overlapPolicy;
        _timeProvider = timeProvider;
    }

    public async Task<Result<ApproverRequestDetail>> HandleAsync(string approverId, Guid requestId, CancellationToken cancellationToken)
    {
        var approver = await _identityService.GetApproverAsync(approverId, cancellationToken);
        if (ApproverPolicies.CanViewQueue(approver) is { } queueError)
        {
            return Result<ApproverRequestDetail>.Failure(queueError);
        }

        var request = _dbContext.VacationRequests.SingleOrDefault(candidate => candidate.Id == requestId);
        if (request is null)
        {
            return Result<ApproverRequestDetail>.Failure(new Error(ErrorCodes.NotFound, "Solicitud no encontrada."));
        }

        if (request.Status == RequestStatus.Pending && ApproverPolicies.CanResolvePending(approver, request) is { } resolveError)
        {
            return Result<ApproverRequestDetail>.Failure(resolveError);
        }

        if (request.Status == RequestStatus.Approved && ApproverPolicies.CanDeactivateApproved(approver, request) is { } deactivateError)
        {
            return Result<ApproverRequestDetail>.Failure(deactivateError);
        }

        if (request.Status is not (RequestStatus.Pending or RequestStatus.Approved))
        {
            return Result<ApproverRequestDetail>.Failure(Error.Conflict("La solicitud ya no es elegible para resolución."));
        }

        var balance = _dbContext.VacationBalances.Single(candidate => candidate.UserId == request.OwnerId);
        var otherRequests = _dbContext.VacationRequests
            .Where(candidate => candidate.OwnerId == request.OwnerId && candidate.Id != request.Id)
            .ToList();
        var hasOverlapWarning = _overlapPolicy.HasBlockingOverlap(otherRequests, request.DateRange);

        var businessDate = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);
        var canDeactivate = request.Status == RequestStatus.Approved && request.StartDate > businessDate;
        var users = await _userDirectory.GetUsersByIdsAsync([request.OwnerId], cancellationToken);
        var requesterName = users.TryGetValue(request.OwnerId, out var requester)
            ? requester.DisplayName
            : request.OwnerId;
        var availableExcludingCurrentRequest = balance.AccruedDays
            - balance.DeductedDays
            - (balance.ReservedDays - request.WorkingDays);

        return Result<ApproverRequestDetail>.Success(new ApproverRequestDetail(
            request.Id,
            request.OwnerId,
            requesterName,
            request.StartDate,
            request.EndDate,
            request.WorkingDays,
            request.Status,
            availableExcludingCurrentRequest,
            availableExcludingCurrentRequest - request.WorkingDays,
            request.Status == RequestStatus.Pending,
            canDeactivate,
            hasOverlapWarning,
            request.RowVersion));
    }
}
