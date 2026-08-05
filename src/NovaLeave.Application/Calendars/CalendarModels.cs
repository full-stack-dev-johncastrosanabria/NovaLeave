using System.Globalization;
using NovaLeave.Domain.Enums;

namespace NovaLeave.Application.Calendars;

public enum RoleContext
{
    User,
    Approver,
    HR
}

public sealed record YearMonth(int Year, int Month)
{
    public DateOnly FirstDay => new(Year, Month, 1);

    public string DisplayName => FirstDay.ToString("MMMM yyyy", CultureInfo.GetCultureInfo("es-GT"));
}

public sealed record CalendarEvent(
    Guid RequestId,
    string Label,
    string? RequesterName,
    DateOnly StartDate,
    DateOnly EndDate,
    int WorkingDays,
    RequestStatus Status,
    bool CanNavigateToDetail)
{
    public string AccessibleName
    {
        get
        {
            var requester = string.IsNullOrWhiteSpace(RequesterName) ? Label : $"{Label} de {RequesterName}";
            return $"{requester}, del {StartDate:yyyy-MM-dd} al {EndDate:yyyy-MM-dd}, {WorkingDays} días hábiles, {StatusLabel}";
        }
    }

    public string StatusLabel => Status switch
    {
        RequestStatus.Pending => "Pendiente",
        RequestStatus.Approved => "Aprobada",
        RequestStatus.Rejected => "Rechazada",
        RequestStatus.CancelledByTimeout => "Cancelada automaticamente",
        RequestStatus.CancelledByApprover => "Cancelada por aprobador",
        _ => Status.ToString()
    };

    public string StatusCssClass => Status switch
    {
        RequestStatus.Pending => "text-warning-emphasis bg-warning-subtle border-warning-subtle",
        RequestStatus.Approved => "text-success-emphasis bg-success-subtle border-success-subtle",
        RequestStatus.Rejected => "text-danger-emphasis bg-danger-subtle border-danger-subtle",
        _ => "text-secondary-emphasis bg-secondary-subtle border-secondary-subtle"
    };

    public string? DetailUrl(string detailRouteTemplate)
    {
        return CanNavigateToDetail ? detailRouteTemplate.Replace("{id}", RequestId.ToString(), StringComparison.Ordinal) : null;
    }
}

public sealed record CalendarDay(DateOnly Date, bool IsCurrentMonth, bool IsToday, bool IsWeekend, IReadOnlyList<CalendarEvent> Events);

public sealed record CalendarViewModel(
    YearMonth CurrentMonth,
    IReadOnlyList<CalendarEvent> Events,
    RoleContext CurrentRole,
    bool CanNavigateToDetail,
    string Title,
    string DetailRouteTemplate,
    IReadOnlyList<CalendarDay> Days)
{
    public static CalendarViewModel Create(
        YearMonth currentMonth,
        RoleContext currentRole,
        string title,
        string detailRouteTemplate,
        IReadOnlyList<CalendarEvent> events,
        DateOnly today)
    {
        var firstGridDay = StartOfWeek(currentMonth.FirstDay);
        var lastMonthDay = currentMonth.FirstDay.AddMonths(1).AddDays(-1);
        var lastGridDay = EndOfWeek(lastMonthDay);
        var dayCount = lastGridDay.DayNumber - firstGridDay.DayNumber + 1;

        var days = Enumerable.Range(0, dayCount)
            .Select(offset =>
            {
                var date = firstGridDay.AddDays(offset);
                var isWeekend = date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
                var dayEvents = isWeekend
                    ? []
                    : events
                        .Where(calendarEvent => calendarEvent.StartDate <= date && calendarEvent.EndDate >= date)
                        .OrderBy(calendarEvent => calendarEvent.StartDate)
                        .ThenBy(calendarEvent => calendarEvent.EndDate)
                        .ToList();

                return new CalendarDay(date, date.Month == currentMonth.Month, date == today, isWeekend, dayEvents);
            })
            .ToList();

        return new CalendarViewModel(currentMonth, events, currentRole, events.Any(static item => item.CanNavigateToDetail), title, detailRouteTemplate, days);
    }

    private static DateOnly StartOfWeek(DateOnly date)
    {
        var offset = date.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)date.DayOfWeek - (int)DayOfWeek.Monday;
        return date.AddDays(-offset);
    }

    private static DateOnly EndOfWeek(DateOnly date)
    {
        var offset = date.DayOfWeek == DayOfWeek.Sunday ? 0 : 7 - (int)date.DayOfWeek;
        return date.AddDays(offset);
    }
}
