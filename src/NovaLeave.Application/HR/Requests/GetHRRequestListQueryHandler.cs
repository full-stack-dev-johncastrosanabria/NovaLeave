using NovaLeave.Application.Common.Interfaces;
using NovaLeave.Application.Common.Models;
using NovaLeave.Domain.Enums;

namespace NovaLeave.Application.HR.Requests;

public sealed record GetHRRequestListQuery(int Page = 1, int PageSize = 50, RequestStatus? Status = null);

public sealed record HRRequestSummary(
    Guid Id,
    string RequesterId,
    string RequesterName,
    DateOnly StartDate,
    DateOnly EndDate,
    int WorkingDays,
    RequestStatus Status,
    DateTime CreatedAtUtc);

public sealed class GetHRRequestListQueryHandler
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserDirectory _userDirectory;

    public GetHRRequestListQueryHandler(IApplicationDbContext dbContext, IUserDirectory userDirectory)
    {
        _dbContext = dbContext;
        _userDirectory = userDirectory;
    }

    public async Task<PagedResult<HRRequestSummary>> HandleAsync(GetHRRequestListQuery query, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Max(1, query.PageSize);
        var requestsQuery = _dbContext.VacationRequests;
        if (query.Status is not null)
        {
            requestsQuery = requestsQuery.Where(request => request.Status == query.Status.Value);
        }

        var totalCount = requestsQuery.Count();
        var requests = requestsQuery
            .OrderByDescending(request => request.CreatedAtUtc)
            .ThenBy(request => request.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var users = await _userDirectory.GetUsersByIdsAsync(requests.Select(request => request.OwnerId).Distinct().ToArray(), cancellationToken);
        var items = requests
            .Select(request => new HRRequestSummary(
                request.Id,
                request.OwnerId,
                ResolveName(users, request.OwnerId),
                request.StartDate,
                request.EndDate,
                request.WorkingDays,
                request.Status,
                request.CreatedAtUtc))
            .ToList();

        return new PagedResult<HRRequestSummary>(items, page, pageSize, totalCount);
    }

    private static string ResolveName(IReadOnlyDictionary<string, UserDirectoryEntry> users, string userId)
    {
        return users.TryGetValue(userId, out var user) ? user.DisplayName : userId;
    }
}
