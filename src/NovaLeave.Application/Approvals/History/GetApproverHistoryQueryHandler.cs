using NovaLeave.Application.Approvals.Models;
using NovaLeave.Application.Authorization;
using NovaLeave.Application.Common.Interfaces;
using NovaLeave.Application.Common.Results;
using NovaLeave.Domain.Enums;

namespace NovaLeave.Application.Approvals.History;

public sealed class GetApproverHistoryQueryHandler
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IApproverIdentityService _identityService;
    private readonly IUserDirectory _userDirectory;

    public GetApproverHistoryQueryHandler(IApplicationDbContext dbContext, IApproverIdentityService identityService, IUserDirectory userDirectory)
    {
        _dbContext = dbContext;
        _identityService = identityService;
        _userDirectory = userDirectory;
    }

    public async Task<Result<IReadOnlyList<ResolutionHistoryItem>>> HandleAsync(string approverId, CancellationToken cancellationToken)
    {
        var approver = await _identityService.GetApproverAsync(approverId, cancellationToken);
        if (ApproverPolicies.CanViewQueue(approver) is { } authorization)
        {
            return Result<IReadOnlyList<ResolutionHistoryItem>>.Failure(authorization);
        }

        var rows = _dbContext.AuditRecords
            .Where(audit => audit.ActorId == approverId && (audit.Action == "Approve" || audit.Action == "Reject" || audit.Action == "Deactivate"))
            .OrderByDescending(audit => audit.TimestampUtc)
            .Take(50)
            .AsEnumerable()
            .Select(audit =>
            {
                var request = _dbContext.VacationRequests.Single(request => request.Id == audit.EntityId);
                return new { Audit = audit, Request = request };
            })
            .ToList();
        var users = await _userDirectory.GetUsersByIdsAsync(rows.Select(item => item.Request.OwnerId).Distinct().ToArray(), cancellationToken);
        var items = rows.Select(item => new ResolutionHistoryItem(
            item.Audit.TimestampUtc,
            item.Audit.Action,
            item.Request.Id,
            item.Request.OwnerId,
            users.TryGetValue(item.Request.OwnerId, out var user) ? user.DisplayName : "Usuario",
            item.Request.StartDate,
            item.Request.EndDate,
            item.Request.WorkingDays,
            item.Request.Status,
            item.Request.RejectionReason)).ToList();

        return Result<IReadOnlyList<ResolutionHistoryItem>>.Success(items);
    }
}
