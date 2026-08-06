using NovaLeave.Application.Common.Results;

namespace NovaLeave.Application.HR.ApproverCapabilities.ToggleApproverCapability;

public sealed class ToggleApproverCapabilityCommandHandler
{
    private readonly IApproverCapabilityStore _store;

    public ToggleApproverCapabilityCommandHandler(IApproverCapabilityStore store)
    {
        _store = store;
    }

    public Task<Result> HandleAsync(ToggleApproverCapabilityCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Reason) || command.Reason.Trim().Length is < 10 or > 500)
        {
            return _store.ToggleAsync(new ToggleApproverCapabilityRequest(
                command.HRUserId,
                command.TargetUserId,
                command.Enable,
                command.Reason,
                command.Confirmed,
                command.ExpectedConcurrencyStamp), cancellationToken);
        }

        if (!command.Confirmed)
        {
            return _store.ToggleAsync(new ToggleApproverCapabilityRequest(
                command.HRUserId,
                command.TargetUserId,
                command.Enable,
                command.Reason,
                command.Confirmed,
                command.ExpectedConcurrencyStamp), cancellationToken);
        }

        return _store.ToggleAsync(new ToggleApproverCapabilityRequest(
            command.HRUserId,
            command.TargetUserId,
            command.Enable,
            command.Reason.Trim(),
            command.Confirmed,
            command.ExpectedConcurrencyStamp), cancellationToken);
    }
}
