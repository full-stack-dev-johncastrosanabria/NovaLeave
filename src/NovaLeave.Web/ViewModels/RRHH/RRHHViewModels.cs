using NovaLeave.Application.Common.Models;
using NovaLeave.Application.Calendars;
using NovaLeave.Application.HR.Audit;
using NovaLeave.Application.HR.Balances;
using NovaLeave.Application.HR.Requests;

namespace NovaLeave.Web.ViewModels.RRHH;

public sealed record RRHHDashboardViewModel(
    int TotalRequests,
    int PendingRequests,
    int ApprovedRequests,
    int UsersWithBalance,
    int RegisteredApprovers,
    int ActiveApprovers);

public sealed record RRHHSolicitudesIndexViewModel(PagedResult<HRRequestSummary> Requests);

public sealed record RRHHSolicitudDetalleViewModel(HRRequestDetail Request);

public sealed record RRHHCalendarioViewModel(CalendarViewModel Calendar);

public sealed record RRHHSaldosIndexViewModel(PagedResult<HRBalanceSummary> Balances);

public sealed record RRHHMovimientosViewModel(HRBalanceMovementView Balance);

public sealed record RRHHAuditoriaViewModel(PagedResult<HRAuditLogItem> Records);
