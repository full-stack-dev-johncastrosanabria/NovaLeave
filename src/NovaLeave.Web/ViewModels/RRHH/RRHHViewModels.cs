using NovaLeave.Application.Common.Models;
using NovaLeave.Application.Calendars;
using NovaLeave.Application.HR.Audit;
using NovaLeave.Application.HR.Balances;
using NovaLeave.Application.HR.Requests;

namespace NovaLeave.Web.ViewModels.RRHH;

/// <summary>
/// Read-only overview of the organization for the HR context. Every figure is derived from the
/// same authorized read queries the detail pages use; HR gains no write capability here
/// (constitution §4.3).
/// </summary>
public sealed record RRHHDashboardViewModel(
    int TotalRequests,
    int PendingRequests,
    int ApprovedRequests,
    int RejectedRequests,
    int CancelledRequests,
    int TotalEmployees,
    int TotalAvailableDays,
    int TotalReservedDays,
    int TotalDeductedDays,
    int ApproverCount,
    int ApproversAbleToResolve,
    IReadOnlyList<HRRequestSummary> RecentRequests,
    IReadOnlyList<HRAuditLogItem> RecentAudit)
{
    /// <summary>Share of resolved requests that were approved, for the approval-rate tile.</summary>
    public int ApprovalRatePercent =>
        ApprovedRequests + RejectedRequests == 0
            ? 0
            : (int)Math.Round(ApprovedRequests * 100.0 / (ApprovedRequests + RejectedRequests));

    public bool HasRequests => TotalRequests > 0;
}

public sealed record RRHHSolicitudesIndexViewModel(PagedResult<HRRequestSummary> Requests);

public sealed record RRHHSolicitudDetalleViewModel(HRRequestDetail Request);

public sealed record RRHHCalendarioViewModel(CalendarViewModel Calendar);

public sealed record RRHHSaldosIndexViewModel(PagedResult<HRBalanceSummary> Balances);

public sealed record RRHHMovimientosViewModel(HRBalanceMovementView Balance);

public sealed record RRHHAuditoriaViewModel(PagedResult<HRAuditLogItem> Records);
