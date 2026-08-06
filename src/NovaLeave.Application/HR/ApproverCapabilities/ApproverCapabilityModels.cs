using NovaLeave.Application.Common.Errors;
using NovaLeave.Application.Common.Results;

namespace NovaLeave.Application.HR.ApproverCapabilities;

public sealed record ApproverCapabilityItem(
    string UserId,
    string DisplayName,
    bool IsActive,
    bool CanResolveRequests,
    string ConcurrencyStamp);

public sealed record ToggleApproverCapabilityRequest(
    string HRUserId,
    string TargetUserId,
    bool Enable,
    string Reason,
    bool Confirmed,
    string ExpectedConcurrencyStamp);

public interface IApproverCapabilityStore
{
    Task<IReadOnlyList<ApproverCapabilityItem>> ListApproversAsync(CancellationToken cancellationToken);

    Task<ApproverCapabilityItem?> GetApproverAsync(string targetUserId, CancellationToken cancellationToken);

    Task<Result> ToggleAsync(ToggleApproverCapabilityRequest request, CancellationToken cancellationToken);
}
