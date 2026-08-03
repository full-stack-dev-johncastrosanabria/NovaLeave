using NovaLeave.Application.Approvals.Models;

namespace NovaLeave.Web.ViewModels.Aprobaciones;

public sealed record AprobacionesIndexViewModel(IReadOnlyList<ApproverRequestSummary> Requests);

public sealed record AprobacionDetalleViewModel(ApproverRequestDetail Request, string RowVersion, string RejectionReason = "");

public sealed record AprobacionesHistorialViewModel(IReadOnlyList<ResolutionHistoryItem> Items);

public sealed class RechazarSolicitudViewModel
{
    public string RejectionReason { get; set; } = string.Empty;

    public string RowVersion { get; set; } = string.Empty;
}

public sealed class DesactivarSolicitudViewModel
{
    public string RowVersion { get; set; } = string.Empty;
}
