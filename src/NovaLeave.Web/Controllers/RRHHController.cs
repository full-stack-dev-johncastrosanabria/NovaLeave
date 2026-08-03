using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLeave.Application.Calendars;
using NovaLeave.Application.Common.Errors;
using NovaLeave.Application.Common.Interfaces;
using NovaLeave.Application.HR.Audit;
using NovaLeave.Application.HR.ApproverCapabilities.ListApproverCapabilities;
using NovaLeave.Application.HR.ApproverCapabilities.ToggleApproverCapability;
using NovaLeave.Application.HR.Balances;
using NovaLeave.Application.HR.Calendar;
using NovaLeave.Application.HR.Requests;
using NovaLeave.Domain.Enums;
using NovaLeave.Web.ViewModels.RRHH;
using NovaLeave.Web.ViewModels.RRHH.ApproverCapabilities;

namespace NovaLeave.Web.Controllers;

[Authorize(Policy = "RequireActiveHR")]
public sealed class RRHHController : Controller
{
    private readonly ICurrentUser _currentUser;
    private readonly GetHRRequestListQueryHandler _getRequests;
    private readonly GetHRRequestDetailQueryHandler _getRequestDetail;
    private readonly GetHRCalendarQueryHandler _getCalendar;
    private readonly GetHRBalancesQueryHandler _getBalances;
    private readonly GetHRBalanceMovementsQueryHandler _getMovements;
    private readonly GetHRAuditLogQueryHandler _getAudit;
    private readonly ListApproverCapabilitiesQueryHandler _listCapabilities;
    private readonly GetApproverCapabilityQueryHandler _getCapability;
    private readonly ToggleApproverCapabilityCommandHandler _toggleCapability;

    public RRHHController(
        ICurrentUser currentUser,
        GetHRRequestListQueryHandler getRequests,
        GetHRRequestDetailQueryHandler getRequestDetail,
        GetHRCalendarQueryHandler getCalendar,
        GetHRBalancesQueryHandler getBalances,
        GetHRBalanceMovementsQueryHandler getMovements,
        GetHRAuditLogQueryHandler getAudit,
        ListApproverCapabilitiesQueryHandler listCapabilities,
        GetApproverCapabilityQueryHandler getCapability,
        ToggleApproverCapabilityCommandHandler toggleCapability)
    {
        _currentUser = currentUser;
        _getRequests = getRequests;
        _getRequestDetail = getRequestDetail;
        _getCalendar = getCalendar;
        _getBalances = getBalances;
        _getMovements = getMovements;
        _getAudit = getAudit;
        _listCapabilities = listCapabilities;
        _getCapability = getCapability;
        _toggleCapability = toggleCapability;
    }

    [HttpGet("/rrhh")]
    public IActionResult Index()
    {
        return View(new RRHHDashboardViewModel());
    }

    [HttpGet("/rrhh/solicitudes")]
    public async Task<IActionResult> Requests(int page = 1, int pageSize = 50, RequestStatus? status = null, CancellationToken cancellationToken = default)
    {
        var result = await _getRequests.HandleAsync(new GetHRRequestListQuery(page, pageSize, status), cancellationToken);
        return View("Solicitudes", new RRHHSolicitudesIndexViewModel(result));
    }

    [HttpGet("/rrhh/solicitudes/{id:guid}")]
    public async Task<IActionResult> RequestDetail(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getRequestDetail.HandleAsync(RequireUserId(), id, cancellationToken);
        return result.IsFailure ? ToActionResult(result.Error) : View("SolicitudDetalle", new RRHHSolicitudDetalleViewModel(result.Value!));
    }

    [HttpGet("/rrhh/calendario")]
    public async Task<IActionResult> Calendar(int? year = null, int? month = null, CancellationToken cancellationToken = default)
    {
        var events = await _getCalendar.HandleAsync(new GetHRCalendarQuery(year, month), cancellationToken);
        var calendarEvents = events
            .Select(item => new CalendarEvent(
                item.RequestId,
                "Solicitud",
                item.RequesterName,
                item.StartDate,
                item.EndDate,
                item.WorkingDays,
                item.Status,
                true))
            .ToList();

        return View("Calendario", new RRHHCalendarioViewModel(CalendarViewModel.Create(
            ResolveMonth(year, month, calendarEvents),
            RoleContext.HR,
            "Calendario RRHH",
            "/rrhh/solicitudes/{id}",
            calendarEvents)));
    }

    [HttpGet("/rrhh/saldos")]
    public async Task<IActionResult> Balances(int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var result = await _getBalances.HandleAsync(new GetHRBalancesQuery(page, pageSize), cancellationToken);
        return View("Saldos", new RRHHSaldosIndexViewModel(result));
    }

    [HttpGet("/rrhh/saldos/{userId}")]
    public async Task<IActionResult> BalanceMovements(string userId, CancellationToken cancellationToken)
    {
        var result = await _getMovements.HandleAsync(userId, cancellationToken);
        return result.IsFailure ? ToActionResult(result.Error) : View("Movimientos", new RRHHMovimientosViewModel(result.Value!));
    }

    [HttpGet("/rrhh/auditoria")]
    public async Task<IActionResult> Audit(int page = 1, int pageSize = 50, [FromQuery(Name = "action")] string? auditAction = null, CancellationToken cancellationToken = default)
    {
        var result = await _getAudit.HandleAsync(new GetHRAuditLogQuery(page, pageSize, auditAction), cancellationToken);
        return View("Auditoria", new RRHHAuditoriaViewModel(result));
    }

    [HttpGet("/rrhh/aprobadores")]
    public async Task<IActionResult> ApproverCapabilities(CancellationToken cancellationToken)
    {
        var approvers = await _listCapabilities.HandleAsync(cancellationToken);
        return View("ApproverCapabilities/Index", new ApproverCapabilitiesIndexViewModel(approvers));
    }

    [HttpGet("/rrhh/aprobadores/{id}/capacidad")]
    public async Task<IActionResult> ApproverCapability(string id, CancellationToken cancellationToken)
    {
        var result = await _getCapability.HandleAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            return ToActionResult(result.Error);
        }

        return View("ApproverCapabilities/Capability", BuildCapabilityForm(result.Value!));
    }

    [HttpPost("/rrhh/aprobadores/{id}/capacidad")]
    public async Task<IActionResult> ToggleApproverCapability(string id, ToggleApproverCapabilityInput input, CancellationToken cancellationToken)
    {
        if (!TryDecode(input.RowVersion, out var rowVersion))
        {
            return BadRequest("RowVersion no valida.");
        }

        var result = await _toggleCapability.HandleAsync(new ToggleApproverCapabilityCommand(
            RequireUserId(),
            id,
            input.Enable,
            input.Reason ?? string.Empty,
            input.Confirmed,
            rowVersion), cancellationToken);

        if (result.IsFailure)
        {
            return ToActionResult(result.Error);
        }

        TempData["Toast"] = "Capacidad de aprobador actualizada.";
        return RedirectToAction(nameof(ApproverCapabilities));
    }

    [HttpPost("/rrhh/saldos/{userId}")]
    [HttpPost("/rrhh/roles/{userId}")]
    [HttpPost("/rrhh/usuarios/{userId}/roles")]
    [HttpPost("/rrhh/aprobadores/{userId}/roles")]
    [HttpPost("/rrhh/aprobadores/{userId}/asignar")]
    [HttpPost("/rrhh/aprobadores/{userId}/remover")]
    public IActionResult DenyForgedMutations(string userId)
    {
        return Forbid();
    }

    private string RequireUserId()
    {
        return _currentUser.UserId ?? throw new InvalidOperationException("Usuario autenticado requerido.");
    }

    private IActionResult ToActionResult(Error? error)
    {
        return error?.Code switch
        {
            ErrorCodes.NotFound => NotFound(),
            ErrorCodes.Forbidden => Forbid(),
            ErrorCodes.Conflict => Conflict(error.Message),
            _ => BadRequest(error?.Message ?? "Solicitud invalida.")
        };
    }

    private static ApproverCapabilityFormViewModel BuildCapabilityForm(NovaLeave.Application.HR.ApproverCapabilities.ApproverCapabilityItem item)
    {
        return new ApproverCapabilityFormViewModel(
            item,
            new ToggleApproverCapabilityInput { Enable = item.CanResolveRequests, RowVersion = Convert.ToBase64String(item.RowVersion) },
            Convert.ToBase64String(item.RowVersion));
    }

    private static bool TryDecode(string rowVersion, out byte[] decoded)
    {
        try
        {
            decoded = Convert.FromBase64String(rowVersion);
            return true;
        }
        catch (FormatException)
        {
            decoded = [];
            return false;
        }
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
