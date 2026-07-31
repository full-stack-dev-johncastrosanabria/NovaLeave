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
    private readonly OverlapPolicy _overlapPolicy;

    public GetApproverRequestDetailQueryHandler(IApplicationDbContext dbContext, IApproverIdentityService identityService, OverlapPolicy overlapPolicy)
    {
        _dbContext = dbContext;
        _identityService = identityService;
        _overlapPolicy = overlapPolicy;
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

        if (ApproverPolicies.CanResolve(approver, request) is { } resolveError)
        {
            return Result<ApproverRequestDetail>.Failure(resolveError);
        }

        var balance = _dbContext.VacationBalances.Single(candidate => candidate.UserId == request.OwnerId);
        var otherRequests = _dbContext.VacationRequests
            .Where(candidate => candidate.OwnerId == request.OwnerId && candidate.Id != request.Id)
            .ToList();
        var hasOverlapWarning = _overlapPolicy.HasBlockingOverlap(otherRequests, request.DateRange);

        return Result<ApproverRequestDetail>.Success(new ApproverRequestDetail(
            request.Id,
            request.OwnerId,
            request.StartDate,
            request.EndDate,
            request.WorkingDays,
            request.Status,
            balance.AvailableDays,
            balance.AccruedDays - balance.DeductedDays - (balance.ReservedDays - request.WorkingDays),
            hasOverlapWarning,
            request.RowVersion));
    }
}
