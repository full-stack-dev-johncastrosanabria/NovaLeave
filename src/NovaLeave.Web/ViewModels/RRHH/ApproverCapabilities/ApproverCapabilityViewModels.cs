using NovaLeave.Application.HR.ApproverCapabilities;

namespace NovaLeave.Web.ViewModels.RRHH.ApproverCapabilities;

public sealed record ApproverCapabilitiesIndexViewModel(IReadOnlyList<ApproverCapabilityItem> Approvers);

public sealed record ApproverCapabilityFormViewModel(ApproverCapabilityItem Approver, ToggleApproverCapabilityInput Input, string ConcurrencyStamp);

public sealed class ToggleApproverCapabilityInput
{
    public bool Enable { get; set; }

    public string? Reason { get; set; }

    public bool Confirmed { get; set; }

    public string ConcurrencyStamp { get; set; } = string.Empty;
}
