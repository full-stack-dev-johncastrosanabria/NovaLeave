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

    public GetApproverQueueQueryHandler(IApplicationDbContext dbContext, IApproverIdentityService identityService)
    {
        _dbContext = dbContext;
        _identityService = identityService;
    }

    public async Task<Result<IReadOnlyList<ApproverRequestSummary>>> HandleAsync(string approverId, CancellationToken cancellationToken)
    {
        var approver = await _identityService.GetApproverAsync(approverId, cancellationToken);
        var authorization = ApproverPolicies.CanViewQueue(approver);
        if (authorization is not null)
        {
            return Result<IReadOnlyList<ApproverRequestSummary>>.Failure(authorization);
        }

        var summaries = _dbContext.VacationRequests
            .Where(request => request.Status == RequestStatus.Pending && request.OwnerId != approverId)
            .OrderBy(request => request.CreatedAtUtc)
            .Take(50)
            .Select(request => new
            {
                Request = request,
                Balance = _dbContext.VacationBalances.Single(balance => balance.UserId == request.OwnerId)
            })
            .AsEnumerable()
            .Select(item => new ApproverRequestSummary(
                item.Request.Id,
                item.Request.OwnerId,
                item.Request.StartDate,
                item.Request.EndDate,
                item.Request.WorkingDays,
                item.Balance.AvailableDays,
                item.Balance.AccruedDays - item.Balance.DeductedDays - (item.Balance.ReservedDays - item.Request.WorkingDays)))
            .ToList();

        return Result<IReadOnlyList<ApproverRequestSummary>>.Success(summaries);
    }
}
