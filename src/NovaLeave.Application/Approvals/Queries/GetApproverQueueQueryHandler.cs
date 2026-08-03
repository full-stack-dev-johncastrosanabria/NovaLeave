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

        var requests = _dbContext.VacationRequests
            .Where(request => request.Status == RequestStatus.Pending && request.OwnerId != approverId)
            .OrderBy(request => request.CreatedAtUtc)
            .Take(50)
            .Select(request => new
            {
                Request = request,
                Balance = _dbContext.VacationBalances.Single(balance => balance.UserId == request.OwnerId)
            })
            .ToList();
        var users = await _userDirectory.GetUsersByIdsAsync(requests.Select(item => item.Request.OwnerId).Distinct().ToArray(), cancellationToken);
        var summaries = requests
            .Select(item => new ApproverRequestSummary(
                item.Request.Id,
                item.Request.OwnerId,
                users.TryGetValue(item.Request.OwnerId, out var user) ? user.DisplayName : "Usuario",
                item.Request.StartDate,
                item.Request.EndDate,
                item.Request.WorkingDays,
                item.Balance.AvailableDays,
                item.Balance.AvailableDays))
            .ToList();

        return Result<IReadOnlyList<ApproverRequestSummary>>.Success(summaries);
    }
}
