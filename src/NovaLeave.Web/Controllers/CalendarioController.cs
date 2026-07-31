using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLeave.Application.Calendars.GetPersonalCalendar;
using NovaLeave.Application.Common.Interfaces;
using NovaLeave.Web.ViewModels.Calendario;

namespace NovaLeave.Web.Controllers;

[Authorize(Policy = "RequireActiveUser")]
public sealed class CalendarioController : Controller
{
    private readonly ICurrentUser _currentUser;
    private readonly GetPersonalCalendarQueryHandler _getPersonalCalendar;

    public CalendarioController(ICurrentUser currentUser, GetPersonalCalendarQueryHandler getPersonalCalendar)
    {
        _currentUser = currentUser;
        _getPersonalCalendar = getPersonalCalendar;
    }

    [HttpGet("/calendario")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new InvalidOperationException("Usuario autenticado requerido.");
        var events = await _getPersonalCalendar.HandleAsync(userId, cancellationToken);
        return View(new CalendarioViewModel(events));
    }
}
