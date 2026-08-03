namespace NovaLeave.Application.HR.ApproverCapabilities.ListApproverCapabilities;

public sealed class ListApproverCapabilitiesQueryHandler
{
    private readonly IApproverCapabilityStore _store;

    public ListApproverCapabilitiesQueryHandler(IApproverCapabilityStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<ApproverCapabilityItem>> HandleAsync(CancellationToken cancellationToken)
    {
        return _store.ListApproversAsync(cancellationToken);
    }
}
