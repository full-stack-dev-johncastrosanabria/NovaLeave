namespace NovaLeave.Application.HR.ApproverCapabilities.ToggleApproverCapability;

public sealed record ToggleApproverCapabilityCommand(
    string HRUserId,
    string TargetUserId,
    bool Enable,
    string Reason,
    bool Confirmed,
    byte[] ExpectedRowVersion);
