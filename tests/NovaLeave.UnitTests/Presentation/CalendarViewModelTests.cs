using NovaLeave.Application.Calendars;
using NovaLeave.Domain.Enums;

namespace NovaLeave.UnitTests.Presentation;

public sealed class CalendarViewModelTests
{
    [Fact]
    public void Calendar_View_Model_Builds_Month_Grid_With_Weekday_Metadata()
    {
        var month = new YearMonth(2027, 2);
        var model = CalendarViewModel.Create(
            month,
            RoleContext.User,
            "Mi calendario",
            "/mis-solicitudes/{id}",
            [
                new CalendarEvent(
                    Guid.NewGuid(),
                    "Solicitud propia",
                    null,
                    new DateOnly(2027, 2, 1),
                    new DateOnly(2027, 2, 3),
                    3,
                    RequestStatus.Approved,
                    true)
            ]);

        Assert.Equal(month, model.CurrentMonth);
        Assert.Equal(0, model.Days.Count % 7);
        Assert.InRange(model.Days.Count / 7, 4, 6);
        Assert.Equal(DayOfWeek.Monday, model.Days.First().Date.DayOfWeek);
        Assert.Contains(model.Days, day => day.Date == new DateOnly(2027, 2, 6) && day.IsWeekend);
        Assert.Contains(model.Days, day => day.Date == new DateOnly(2027, 2, 1) && day.Events.Count == 1);
        Assert.All(model.Days.Where(day => day.IsWeekend), day => Assert.Empty(day.Events));
    }

    [Fact]
    public void Calendar_Event_Uses_Authorized_Detail_Link_When_Allowed()
    {
        var eventId = Guid.NewGuid();
        var item = new CalendarEvent(
            eventId,
            "Solicitud aprobada",
            "user1@example.test",
            new DateOnly(2027, 3, 8),
            new DateOnly(2027, 3, 9),
            2,
            RequestStatus.Approved,
            true);

        Assert.Equal($"/mis-solicitudes/{eventId}", item.DetailUrl("/mis-solicitudes/{id}"));
        Assert.Contains("Solicitud aprobada", item.AccessibleName);
        Assert.Contains("2 dias habiles", item.AccessibleName);
    }

    [Fact]
    public void Calendar_Event_Omits_Detail_Link_When_Role_Is_Not_Authorized()
    {
        var item = new CalendarEvent(
            Guid.NewGuid(),
            "Evento aprobado",
            null,
            new DateOnly(2027, 3, 8),
            new DateOnly(2027, 3, 9),
            2,
            RequestStatus.Approved,
            false);

        Assert.Null(item.DetailUrl("/aprobaciones/{id}"));
        Assert.DoesNotContain("user1@example.test", item.AccessibleName);
    }
}
