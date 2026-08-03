using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLeave.Application.Calendars;
using NovaLeave.Application.Calendars.GetPersonalCalendar;
using NovaLeave.Application.Calendars.GetApproverCalendar;
using NovaLeave.Application.Common.Interfaces;
using NovaLeave.Domain.Enums;
using NovaLeave.Web.ViewModels.Calendario;

namespace NovaLeave.Web.Controllers;

[Authorize]
public sealed class CalendarioController : Controller
{
    private readonly ICurrentUser _currentUser;
    private readonly GetPersonalCalendarQueryHandler _getPersonalCalendar;
    private readonly GetApproverCalendarQueryHandler _getApproverCalendar;

    public CalendarioController(ICurrentUser currentUser, GetPersonalCalendarQueryHandler getPersonalCalendar, GetApproverCalendarQueryHandler getApproverCalendar)
    {
        _currentUser = currentUser;
        _getPersonalCalendar = getPersonalCalendar;
        _getApproverCalendar = getApproverCalendar;
    }

    [HttpGet("/calendario")]
    public async Task<IActionResult> Index(string? context = null, int? year = null, int? month = null, CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId ?? throw new InvalidOperationException("Usuario autenticado requerido.");
        if (User.IsInRole("HR"))
        {
            return Forbid();
        }

        var selectedContext = ResolveContext(context);
        if (selectedContext == RoleContext.Approver)
        {
            if (!User.IsInRole("Approver") || !User.HasClaim("CanResolveRequests", "true"))
            {
                return Forbid();
            }

            var approverResult = await _getApproverCalendar.HandleAsync(userId, User.IsInRole("User"), cancellationToken);
            if (approverResult.IsFailure)
            {
                return Forbid();
            }

            var approverEvents = approverResult.Value!
                .Select(item => new CalendarEvent(
                    item.RequestId,
                    item.Category switch
                    {
                        CalendarEventCategory.OwnRequest => "Mi solicitud",
                        CalendarEventCategory.PendingApproval => "Pendiente de aprobacion",
                        CalendarEventCategory.ResolvedByMe => "Resuelta por mi",
                        _ => "Evento aprobado"
                    },
                    null,
                    item.StartDate,
                    item.EndDate,
                    item.WorkingDays,
                    item.Status,
                    item.CanNavigateToDetail,
                    item.Category,
                    item.Category == CalendarEventCategory.OwnRequest ? "/mis-solicitudes/{id}" : "/aprobaciones/{id}"))
                .ToList();

            return View("Approver", new CalendarioAprobadorViewModel(CalendarViewModel.Create(
                ResolveMonth(year, month, approverEvents),
                RoleContext.Approver,
                "Calendario de aprobaciones",
                "/aprobaciones/{id}",
                approverEvents)));
        }

        if (!User.IsInRole("User"))
        {
            return Forbid();
        }

        var events = await _getPersonalCalendar.HandleAsync(userId, cancellationToken);
        var userEvents = events
            .Select(item => new CalendarEvent(
                item.RequestId,
                "Solicitud propia",
                null,
                item.StartDate,
                item.EndDate,
                item.WorkingDays,
                item.Status,
                true,
                CalendarEventCategory.OwnRequest))
            .ToList();

        return View(new CalendarioViewModel(CalendarViewModel.Create(
            ResolveMonth(year, month, userEvents),
            RoleContext.User,
            "Mi calendario",
            "/mis-solicitudes/{id}",
            userEvents)));
    }

    private RoleContext ResolveContext(string? context)
    {
        if (string.Equals(context, "Approver", StringComparison.OrdinalIgnoreCase))
        {
            return RoleContext.Approver;
        }

        if (string.Equals(context, "User", StringComparison.OrdinalIgnoreCase))
        {
            return RoleContext.User;
        }

        return User.IsInRole("Approver") && !User.IsInRole("User")
            ? RoleContext.Approver
            : RoleContext.User;
    }

    private static YearMonth ResolveMonth(int? year, int? month, IReadOnlyList<CalendarEvent> events)
    {
        if (year is null && month is null && events.Count > 0)
        {
            var firstEvent = events.OrderBy(item => item.StartDate).First();
            return new YearMonth(firstEvent.StartDate.Year, firstEvent.StartDate.Month);
        }

        var now = DateTime.UtcNow;
        return new YearMonth(year ?? now.Year, month ?? now.Month);
    }
}
