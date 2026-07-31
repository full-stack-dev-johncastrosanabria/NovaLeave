using NovaLeave.Application.Calendars.GetPersonalCalendar;

namespace NovaLeave.Web.ViewModels.Calendario;

public sealed record CalendarioViewModel(IReadOnlyList<PersonalCalendarEvent> Events);
