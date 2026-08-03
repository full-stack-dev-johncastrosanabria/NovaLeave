using NovaLeave.Application.Authorization;
using NovaLeave.Application.Common.Errors;
using NovaLeave.Application.Common.Interfaces;
using NovaLeave.Application.Common.Results;
using NovaLeave.Application.Observability;
using NovaLeave.Domain.Entities;

namespace NovaLeave.Application.Approvals.RejectRequest;

public sealed class RejectRequestHandler
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IApproverIdentityService _identityService;
    private readonly TimeProvider _timeProvider;
    private readonly IOperationalTelemetry _telemetry;

    public RejectRequestHandler(
        IApplicationDbContext dbContext,
        IApproverIdentityService identityService,
        TimeProvider timeProvider,
        IOperationalTelemetry telemetry)
    {
        _dbContext = dbContext;
        _identityService = identityService;
        _timeProvider = timeProvider;
        _telemetry = telemetry;
    }

    public async Task<Result> HandleAsync(RejectRequestCommand command, CancellationToken cancellationToken)
    {
        var reason = command.RejectionReason.Trim();
        if (reason.Length is < 10 or > 500)
        {
            return Result.Failure(Error.Validation("La razon de rechazo debe tener entre 10 y 500 caracteres."));
        }

        var approver = await _identityService.GetApproverAsync(command.ApproverId, cancellationToken);
        var request = _dbContext.VacationRequests.SingleOrDefault(candidate => candidate.Id == command.RequestId);
        if (request is null)
        {
            return Result.Failure(new Error(ErrorCodes.NotFound, "Solicitud no encontrada."));
        }

        if (ApproverPolicies.CanResolve(approver, request) is { } authorization)
        {
            return Result.Failure(authorization);
        }

        if (!request.RowVersion.SequenceEqual(command.RowVersion))
        {
            _telemetry.RecordConcurrencyConflict("rejection.rowversion");
            return Result.Failure(Error.Conflict("La solicitud fue modificada por otro proceso."));
        }

        try
        {
            var timestamp = _timeProvider.GetUtcNow();
            request.Reject(command.ApproverId, reason, timestamp);
            var balance = _dbContext.VacationBalances.Single(candidate => candidate.UserId == request.OwnerId);
            var movement = balance.Release(request.Id, request.WorkingDays, command.ApproverId, timestamp);
            _dbContext.AddBalanceMovement(movement);
            _dbContext.AddAuditRecord(AuditRecord.Create(
                command.ApproverId,
                "Approver",
                "Reject",
                nameof(VacationRequest),
                request.Id,
                "Success",
                Guid.NewGuid(),
                request.Id,
                $"{{\"WorkingDays\":{request.WorkingDays}}}",
                timestamp));

            await _dbContext.SaveChangesAsync(cancellationToken);
            _telemetry.RecordRejection(true);
            return Result.Success();
        }
        catch (InvalidOperationException exception)
        {
            _telemetry.RecordRejection(false);
            return Result.Failure(Error.Conflict(exception.Message));
        }
        catch (Exception exception) when (exception.GetType().Name == "DbUpdateConcurrencyException")
        {
            _telemetry.RecordRejection(false);
            _telemetry.RecordConcurrencyConflict("rejection.dbupdate");
            return Result.Failure(Error.Conflict("La solicitud fue modificada por otro proceso."));
        }
    }
}
