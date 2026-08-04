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

        // The shared calendar carries no HR behaviour and must never expose organization-wide
        // data, so an HR-only identity is denied and directed to /rrhh/calendario (RBFV 7.3).
        // An identity that *also* holds User or Approver still reaches its own personal or
        // anonymized approver scope here, which RBFV 5.2 authorizes for those roles.
        if (User.IsInRole("HR") && !User.IsInRole("User") && !User.IsInRole("Approver"))
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

            var approverResult = await _getApproverCalendar.HandleAsync(userId, cancellationToken);
            if (approverResult.IsFailure)
            {
                return Forbid();
            }

            var approverEvents = approverResult.Value!
                .Select(item => new CalendarEvent(
                    item.RequestId,
                    "Evento aprobado",
                    null,
                    item.StartDate,
                    item.EndDate,
                    item.WorkingDays,
                    RequestStatus.Approved,
                    false))
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
                item.RequesterName,
                item.StartDate,
                item.EndDate,
                item.WorkingDays,
                RequestStatus.Approved,
                true))
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
