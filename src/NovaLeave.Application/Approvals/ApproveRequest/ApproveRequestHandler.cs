using NovaLeave.Application.Authorization;
using NovaLeave.Application.Common.Errors;
using NovaLeave.Application.Common.Interfaces;
using NovaLeave.Application.Common.Results;
using NovaLeave.Application.Observability;
using NovaLeave.Domain.Entities;
using NovaLeave.Domain.Services;

namespace NovaLeave.Application.Approvals.ApproveRequest;

public sealed class ApproveRequestHandler
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IApproverIdentityService _identityService;
    private readonly OverlapPolicy _overlapPolicy;
    private readonly TimeProvider _timeProvider;
    private readonly IOperationalTelemetry _telemetry;

    public ApproveRequestHandler(
        IApplicationDbContext dbContext,
        IApproverIdentityService identityService,
        OverlapPolicy overlapPolicy,
        TimeProvider timeProvider,
        IOperationalTelemetry telemetry)
    {
        _dbContext = dbContext;
        _identityService = identityService;
        _overlapPolicy = overlapPolicy;
        _timeProvider = timeProvider;
        _telemetry = telemetry;
    }

    public async Task<Result> HandleAsync(ApproveRequestCommand command, CancellationToken cancellationToken)
    {
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
            _telemetry.RecordConcurrencyConflict("approval.rowversion");
            return Result.Failure(Error.Conflict("La solicitud fue modificada por otro proceso."));
        }

        var otherRequests = _dbContext.VacationRequests
            .Where(candidate => candidate.OwnerId == request.OwnerId && candidate.Id != request.Id)
            .ToList();
        if (_overlapPolicy.HasBlockingOverlap(otherRequests, request.DateRange))
        {
            return Result.Failure(Error.Conflict("La solicitud se traslapa con otra solicitud pendiente o aprobada."));
        }

        var balance = _dbContext.VacationBalances.Single(candidate => candidate.UserId == request.OwnerId);
        // Approval converts this request's reservation into a deduction, so authoritative
        // availability after approval equals current availability including all reservations.
        var projectedBalance = balance.AvailableDays;
        if (projectedBalance < 0)
        {
            return Result.Failure(Error.Conflict("El saldo proyectado no puede ser negativo."));
        }

        try
        {
            var timestamp = _timeProvider.GetUtcNow();
            request.Approve(command.ApproverId, timestamp);
            var movement = balance.Deduct(request.Id, request.WorkingDays, command.ApproverId, timestamp);
            _dbContext.AddBalanceMovement(movement);
            _dbContext.AddAuditRecord(AuditRecord.Create(
                command.ApproverId,
                "Approver",
                "Approve",
                nameof(VacationRequest),
                request.Id,
                "Success",
                Guid.NewGuid(),
                request.Id,
                $"{{\"WorkingDays\":{request.WorkingDays},\"ProjectedBalanceAfterApproval\":{projectedBalance}}}",
                timestamp));

            await _dbContext.SaveChangesAsync(cancellationToken);
            _telemetry.RecordApproval(true);
            return Result.Success();
        }
        catch (InvalidOperationException exception)
        {
            _telemetry.RecordApproval(false);
            return Result.Failure(Error.Conflict(exception.Message));
        }
        catch (Exception exception) when (exception.GetType().Name == "DbUpdateConcurrencyException")
        {
            _telemetry.RecordApproval(false);
            _telemetry.RecordConcurrencyConflict("approval.dbupdate");
            return Result.Failure(Error.Conflict("La solicitud fue modificada por otro proceso."));
        }
    }
}
