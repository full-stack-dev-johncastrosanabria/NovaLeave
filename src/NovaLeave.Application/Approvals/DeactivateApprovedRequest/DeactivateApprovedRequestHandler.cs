using NovaLeave.Application.Authorization;
using NovaLeave.Application.Common.Errors;
using NovaLeave.Application.Common.Interfaces;
using NovaLeave.Application.Common.Results;
using NovaLeave.Domain.Entities;
using NovaLeave.Domain.Services;

namespace NovaLeave.Application.Approvals.DeactivateApprovedRequest;

public sealed class DeactivateApprovedRequestHandler
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IApproverIdentityService _identityService;
    private readonly TimeProvider _timeProvider;

    public DeactivateApprovedRequestHandler(IApplicationDbContext dbContext, IApproverIdentityService identityService, TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _identityService = identityService;
        _timeProvider = timeProvider;
    }

    public async Task<Result> HandleAsync(DeactivateApprovedRequestCommand command, CancellationToken cancellationToken)
    {
        var approver = await _identityService.GetApproverAsync(command.ApproverId, cancellationToken);
        var request = _dbContext.VacationRequests.SingleOrDefault(candidate => candidate.Id == command.RequestId);
        if (request is null)
        {
            return Result.Failure(new Error(ErrorCodes.NotFound, "Solicitud no encontrada."));
        }

        if (ApproverPolicies.CanDeactivateApproved(approver, request) is { } authorization)
        {
            return Result.Failure(authorization);
        }

        if (!request.RowVersion.SequenceEqual(command.RowVersion))
        {
            return Result.Failure(Error.Conflict("La solicitud fue modificada por otro proceso."));
        }

        try
        {
            var timestamp = _timeProvider.GetUtcNow();
            var businessDate = CostaRicaTime.GetBusinessDate(timestamp);
            request.CancelByApprover(command.ApproverId, businessDate, timestamp);
            var balance = _dbContext.VacationBalances.Single(candidate => candidate.UserId == request.OwnerId);
            var movement = balance.Restore(request.Id, request.WorkingDays, command.ApproverId, timestamp);
            _dbContext.AddBalanceMovement(movement);
            _dbContext.AddAuditRecord(AuditRecord.Create(
                command.ApproverId,
                "Approver",
                "Deactivate",
                nameof(VacationRequest),
                request.Id,
                "Success",
                Guid.NewGuid(),
                request.Id,
                $"{{\"WorkingDays\":{request.WorkingDays},\"RestoredDays\":{request.WorkingDays}}}",
                timestamp));

            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (InvalidOperationException exception)
        {
            return Result.Failure(Error.Conflict(exception.Message));
        }
        catch (Exception exception) when (exception.GetType().Name == "DbUpdateConcurrencyException")
        {
            return Result.Failure(Error.Conflict("La solicitud fue modificada por otro proceso."));
        }
    }
}
