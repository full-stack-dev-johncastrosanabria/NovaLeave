using NovaLeave.Application.Common.Interfaces;
using NovaLeave.Domain.Enums;

namespace NovaLeave.Application.Calendars.GetPersonalCalendar;

public sealed record PersonalCalendarEvent(Guid RequestId, string RequesterName, DateOnly StartDate, DateOnly EndDate, int WorkingDays);

public sealed class GetPersonalCalendarQueryHandler
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserDirectory _userDirectory;

    public GetPersonalCalendarQueryHandler(IApplicationDbContext dbContext, IUserDirectory userDirectory)
    {
        _dbContext = dbContext;
        _userDirectory = userDirectory;
    }

    public async Task<IReadOnlyList<PersonalCalendarEvent>> HandleAsync(string userId, CancellationToken cancellationToken)
    {
        var requests = _dbContext.VacationRequests
            .Where(request => request.OwnerId == userId && request.Status == RequestStatus.Approved)
            .OrderBy(request => request.StartDate)
            .ToList();

        var users = await _userDirectory.GetUsersByIdsAsync([userId], cancellationToken);
        var requesterName = users.TryGetValue(userId, out var user) ? user.DisplayName : userId;
        var events = requests
            .Select(request => new PersonalCalendarEvent(request.Id, requesterName, request.StartDate, request.EndDate, request.WorkingDays))
            .ToList();

        return events;
    }
}
