using NovaLeave.Application.Common.Interfaces;
using NovaLeave.Domain.Enums;

namespace NovaLeave.Application.Calendars.GetPersonalCalendar;

public sealed record PersonalCalendarEvent(Guid RequestId, DateOnly StartDate, DateOnly EndDate, int WorkingDays);

public sealed class GetPersonalCalendarQueryHandler
{
    private readonly IApplicationDbContext _dbContext;

    public GetPersonalCalendarQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<PersonalCalendarEvent>> HandleAsync(string userId, CancellationToken cancellationToken)
    {
        var events = _dbContext.VacationRequests
            .Where(request => request.OwnerId == userId && request.Status == RequestStatus.Approved)
            .OrderBy(request => request.StartDate)
            .Select(request => new PersonalCalendarEvent(request.Id, request.StartDate, request.EndDate, request.WorkingDays))
            .ToList();

        return await Task.FromResult(events);
    }
}
