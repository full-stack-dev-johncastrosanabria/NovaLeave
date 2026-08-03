using NovaLeave.Application.Common.Errors;
using NovaLeave.Application.Common.Results;

namespace NovaLeave.Application.HR.ApproverCapabilities.ListApproverCapabilities;

public sealed class GetApproverCapabilityQueryHandler
{
    private readonly IApproverCapabilityStore _store;

    public GetApproverCapabilityQueryHandler(IApproverCapabilityStore store)
    {
        _store = store;
    }

    public async Task<Result<ApproverCapabilityItem>> HandleAsync(string targetUserId, CancellationToken cancellationToken)
    {
        var item = await _store.GetApproverAsync(targetUserId, cancellationToken);
        return item is null
            ? Result<ApproverCapabilityItem>.Failure(new Error(ErrorCodes.NotFound, "Aprobador no encontrado."))
            : Result<ApproverCapabilityItem>.Success(item);
    }
}
