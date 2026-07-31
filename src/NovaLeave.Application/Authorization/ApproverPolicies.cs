using NovaLeave.Application.Common.Errors;
using NovaLeave.Domain.Entities;
using NovaLeave.Domain.Enums;

namespace NovaLeave.Application.Authorization;

public sealed record ApproverIdentity(string UserId, bool IsActive, bool CanResolveRequests);

public interface IApproverIdentityService
{
    Task<ApproverIdentity?> GetApproverAsync(string userId, CancellationToken cancellationToken);
}

public static class ApproverPolicies
{
    public static Error? CanViewQueue(ApproverIdentity? approver)
    {
        return approver is { IsActive: true, CanResolveRequests: true }
            ? null
            : Error.Forbidden("El aprobador no esta habilitado para resolver solicitudes.");
    }

    public static Error? CanResolve(ApproverIdentity? approver, VacationRequest request)
    {
        var queueError = CanViewQueue(approver);
        if (queueError is not null)
        {
            return queueError;
        }

        if (request.OwnerId == approver!.UserId)
        {
            return new Error(ErrorCodes.NotFound, "Solicitud no encontrada.");
        }

        if (request.Status != RequestStatus.Pending)
        {
            return Error.Conflict("La solicitud ya no esta pendiente.");
        }

        return null;
    }
}
