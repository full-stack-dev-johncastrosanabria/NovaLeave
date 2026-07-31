using NovaLeave.Application.Calendars.GetPersonalCalendar;
using NovaLeave.Application.Calendars.GetApproverCalendar;

namespace NovaLeave.Web.ViewModels.Calendario;

public sealed record CalendarioViewModel(IReadOnlyList<PersonalCalendarEvent> Events);

public sealed record CalendarioAprobadorViewModel(IReadOnlyList<ApproverCalendarEvent> Events);
