using NovaLeave.Application.Common.Interfaces;
using NovaLeave.Application.Common.Models;

namespace NovaLeave.Application.HR.Audit;

public sealed record GetHRAuditLogQuery(int Page = 1, int PageSize = 50, string? Action = null);

public sealed record HRAuditLogItem(
    DateTime TimestampUtc,
    string ActorId,
    string ActorRole,
    string Action,
    string EntityType,
    Guid EntityId,
    string Result,
    string? Data);

public sealed class GetHRAuditLogQueryHandler
{
    private readonly IApplicationDbContext _dbContext;

    public GetHRAuditLogQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<PagedResult<HRAuditLogItem>> HandleAsync(GetHRAuditLogQuery query, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Max(1, query.PageSize);
        var auditQuery = _dbContext.AuditRecords;
        if (!string.IsNullOrWhiteSpace(query.Action))
        {
            auditQuery = auditQuery.Where(record => record.Action == query.Action);
        }

        var totalCount = auditQuery.Count();
        var items = auditQuery
            .OrderByDescending(record => record.TimestampUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(record => new HRAuditLogItem(
                record.TimestampUtc,
                record.ActorId,
                record.ActorRole,
                record.Action,
                record.EntityType,
                record.EntityId,
                record.Result,
                record.Data))
            .ToList();

        return Task.FromResult(new PagedResult<HRAuditLogItem>(items, page, pageSize, totalCount));
    }
}
