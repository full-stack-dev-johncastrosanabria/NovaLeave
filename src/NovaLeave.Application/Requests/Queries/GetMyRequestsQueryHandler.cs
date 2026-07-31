using NovaLeave.Application.Common.Interfaces;
using NovaLeave.Application.Requests.Models;

namespace NovaLeave.Application.Requests.Queries;

public sealed class GetMyRequestsQueryHandler
{
    private readonly IApplicationDbContext _dbContext;

    public GetMyRequestsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<UserRequestSummary>> HandleAsync(string userId, CancellationToken cancellationToken)
    {
        var requests = _dbContext.VacationRequests
            .Where(request => request.OwnerId == userId)
            .OrderByDescending(request => request.CreatedAtUtc)
            .Select(request => new UserRequestSummary(
                request.Id,
                request.StartDate,
                request.EndDate,
                request.WorkingDays,
                request.Status,
                request.CreatedAtUtc))
            .ToList();

        return await Task.FromResult(requests);
    }
}
