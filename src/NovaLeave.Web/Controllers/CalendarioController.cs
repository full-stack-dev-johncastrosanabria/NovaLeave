using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLeave.Application.Calendars.GetPersonalCalendar;
using NovaLeave.Application.Calendars.GetApproverCalendar;
using NovaLeave.Application.Common.Interfaces;
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
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new InvalidOperationException("Usuario autenticado requerido.");
        if (User.IsInRole("Approver") && User.HasClaim("CanResolveRequests", "true"))
        {
            var approverResult = await _getApproverCalendar.HandleAsync(userId, cancellationToken);
            if (approverResult.IsFailure)
            {
                return Forbid();
            }

            return View("Approver", new CalendarioAprobadorViewModel(approverResult.Value!));
        }

        var events = await _getPersonalCalendar.HandleAsync(userId, cancellationToken);
        return View(new CalendarioViewModel(events));
    }
}
