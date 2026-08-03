using NovaLeave.Application.Common.Interfaces;
using NovaLeave.Application.Common.Models;

namespace NovaLeave.Application.HR.Audit;

public sealed record GetHRAuditLogQuery(int Page = 1, int PageSize = 50, string? Action = null);

public sealed record HRAuditLogItem(
    DateTime TimestampUtc,
    string ActorId,
    string ActorName,
    string ActorRole,
    string Action,
    string EntityType,
    Guid EntityId,
    string Result,
    string? Data);

public sealed class GetHRAuditLogQueryHandler
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserDirectory _userDirectory;

    public GetHRAuditLogQueryHandler(IApplicationDbContext dbContext, IUserDirectory userDirectory)
    {
        _dbContext = dbContext;
        _userDirectory = userDirectory;
    }

    public async Task<PagedResult<HRAuditLogItem>> HandleAsync(GetHRAuditLogQuery query, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Max(1, query.PageSize);
        var auditQuery = _dbContext.AuditRecords;
        if (!string.IsNullOrWhiteSpace(query.Action))
        {
            auditQuery = auditQuery.Where(record => record.Action == query.Action);
        }

        var totalCount = auditQuery.Count();
        var records = auditQuery
            .OrderByDescending(record => record.TimestampUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        var users = await _userDirectory.GetUsersByIdsAsync(records.Select(record => record.ActorId).Distinct().ToArray(), cancellationToken);
        var items = records.Select(record => new HRAuditLogItem(
                record.TimestampUtc,
                record.ActorId,
                users.TryGetValue(record.ActorId, out var user) ? user.DisplayName : record.ActorRole,
                record.ActorRole,
                record.Action,
                record.EntityType,
                record.EntityId,
                record.Result,
                record.Data))
            .ToList();

        return new PagedResult<HRAuditLogItem>(items, page, pageSize, totalCount);
    }
}
