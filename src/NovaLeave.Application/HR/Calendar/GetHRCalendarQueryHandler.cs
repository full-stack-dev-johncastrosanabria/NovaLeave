using NovaLeave.Application.Common.Interfaces;
using NovaLeave.Domain.Enums;

namespace NovaLeave.Application.HR.Calendar;

public sealed record GetHRCalendarQuery(int? Year = null, int? Month = null);

public sealed record HRCalendarEvent(
    Guid RequestId,
    string RequesterId,
    string RequesterName,
    DateOnly StartDate,
    DateOnly EndDate,
    int WorkingDays,
    RequestStatus Status);

public sealed class GetHRCalendarQueryHandler
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserDirectory _userDirectory;

    public GetHRCalendarQueryHandler(IApplicationDbContext dbContext, IUserDirectory userDirectory)
    {
        _dbContext = dbContext;
        _userDirectory = userDirectory;
    }

    public async Task<IReadOnlyList<HRCalendarEvent>> HandleAsync(GetHRCalendarQuery query, CancellationToken cancellationToken)
    {
        var requestsQuery = _dbContext.VacationRequests;
        if (query.Year is not null && query.Month is not null)
        {
            var monthStart = new DateOnly(query.Year.Value, query.Month.Value, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            requestsQuery = requestsQuery.Where(request => request.StartDate <= monthEnd && request.EndDate >= monthStart);
        }

        var requests = requestsQuery
            .OrderBy(request => request.StartDate)
            .ThenBy(request => request.EndDate)
            .ToList();

        var users = await _userDirectory.GetUsersByIdsAsync(requests.Select(request => request.OwnerId).Distinct().ToArray(), cancellationToken);
        return requests
            .Select(request => new HRCalendarEvent(
                request.Id,
                request.OwnerId,
                users.TryGetValue(request.OwnerId, out var user) ? user.DisplayName : request.OwnerId,
                request.StartDate,
                request.EndDate,
                request.WorkingDays,
                request.Status))
            .ToList();
    }
}
