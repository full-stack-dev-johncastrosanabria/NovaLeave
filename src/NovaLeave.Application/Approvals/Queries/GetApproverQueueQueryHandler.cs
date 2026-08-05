using NovaLeave.Application.Approvals.Models;
using NovaLeave.Application.Authorization;
using NovaLeave.Application.Common.Errors;
using NovaLeave.Application.Common.Interfaces;
using NovaLeave.Application.Common.Results;
using NovaLeave.Domain.Enums;

namespace NovaLeave.Application.Approvals.Queries;

public sealed class GetApproverQueueQueryHandler
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IApproverIdentityService _identityService;
    private readonly IUserDirectory _userDirectory;

    public GetApproverQueueQueryHandler(IApplicationDbContext dbContext, IApproverIdentityService identityService, IUserDirectory userDirectory)
    {
        _dbContext = dbContext;
        _identityService = identityService;
        _userDirectory = userDirectory;
    }

    public async Task<Result<IReadOnlyList<ApproverRequestSummary>>> HandleAsync(string approverId, CancellationToken cancellationToken)
    {
        var approver = await _identityService.GetApproverAsync(approverId, cancellationToken);
        var authorization = ApproverPolicies.CanViewQueue(approver);
        if (authorization is not null)
        {
            return Result<IReadOnlyList<ApproverRequestSummary>>.Failure(authorization);
        }

        var queueItems = _dbContext.VacationRequests
            .Where(request => request.Status == RequestStatus.Pending && request.OwnerId != approverId)
            .OrderBy(request => request.CreatedAtUtc)
            .Take(50)
            .Select(request => new
            {
                Request = request,
                Balance = _dbContext.VacationBalances.Single(balance => balance.UserId == request.OwnerId)
            })
            .AsEnumerable()
            .Select(item =>
            {
                var availableExcludingCurrentRequest = item.Balance.AccruedDays
                    - item.Balance.DeductedDays
                    - (item.Balance.ReservedDays - item.Request.WorkingDays);

                return new
                {
                    item.Request.Id,
                    RequesterId = item.Request.OwnerId,
                    item.Request.StartDate,
                    item.Request.EndDate,
                    item.Request.WorkingDays,
                    AvailableDays = availableExcludingCurrentRequest,
                    ProjectedBalanceAfterApproval = availableExcludingCurrentRequest - item.Request.WorkingDays
                };
            })
            .ToList();

        var users = await _userDirectory.GetUsersByIdsAsync(
            queueItems.Select(item => item.RequesterId).Distinct().ToArray(),
            cancellationToken);
        var summaries = queueItems.Select(item => new ApproverRequestSummary(
            item.Id,
            item.RequesterId,
            users.TryGetValue(item.RequesterId, out var requester) ? requester.DisplayName : item.RequesterId,
            item.StartDate,
            item.EndDate,
            item.WorkingDays,
            item.AvailableDays,
            item.ProjectedBalanceAfterApproval)).ToList();
        return Result<IReadOnlyList<ApproverRequestSummary>>.Success(summaries);
    }
}
