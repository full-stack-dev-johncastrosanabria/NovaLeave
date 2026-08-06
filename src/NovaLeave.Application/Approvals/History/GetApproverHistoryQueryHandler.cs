using NovaLeave.Application.Approvals.Models;
using NovaLeave.Application.Authorization;
using NovaLeave.Application.Common.Interfaces;
using NovaLeave.Application.Common.Results;
using NovaLeave.Domain.Entities;
using NovaLeave.Domain.Enums;

namespace NovaLeave.Application.Approvals.History;

public sealed class GetApproverHistoryQueryHandler
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IApproverIdentityService _identityService;

    public GetApproverHistoryQueryHandler(IApplicationDbContext dbContext, IApproverIdentityService identityService)
    {
        _dbContext = dbContext;
        _identityService = identityService;
    }

    public async Task<Result<IReadOnlyList<ResolutionHistoryItem>>> HandleAsync(string approverId, CancellationToken cancellationToken)
    {
        var approver = await _identityService.GetApproverAsync(approverId, cancellationToken);
        if (ApproverPolicies.CanViewQueue(approver) is { } authorization)
        {
            return Result<IReadOnlyList<ResolutionHistoryItem>>.Failure(authorization);
        }

        var audits = _dbContext.AuditRecords
            .Where(audit => audit.ActorId == approverId &&
                            audit.EntityType == nameof(VacationRequest) &&
                            (audit.Action == "Approve" || audit.Action == "Reject" || audit.Action == "Deactivate"))
            .OrderByDescending(audit => audit.TimestampUtc)
            .Take(50)
            .ToList();

        var requestIds = audits
            .Select(audit => audit.EntityId)
            .Distinct()
            .ToList();

        var requestsById = _dbContext.VacationRequests
            .Where(request => requestIds.Contains(request.Id))
            .ToDictionary(request => request.Id);

        var items = audits
            .Where(audit => requestsById.ContainsKey(audit.EntityId))
            .Select(audit =>
            {
                var request = requestsById[audit.EntityId];
                return new ResolutionHistoryItem(audit.TimestampUtc, audit.Action, request.Id, request.OwnerId, request.Status);
            })
            .ToList();

        return Result<IReadOnlyList<ResolutionHistoryItem>>.Success(items);
    }
}
