using NovaLeave.Application.Common.Errors;
using NovaLeave.Application.Common.Interfaces;
using NovaLeave.Application.Common.Results;
using NovaLeave.Domain.Entities;
using NovaLeave.Domain.Enums;
using NovaLeave.Domain.Services;
using NovaLeave.Domain.ValueObjects;

namespace NovaLeave.Application.Requests.EditPendingRequest;

public sealed class EditPendingRequestHandler
{
    private readonly IApplicationDbContext _dbContext;
    private readonly TimeProvider _timeProvider;
    private readonly OverlapPolicy _overlapPolicy;

    public EditPendingRequestHandler(IApplicationDbContext dbContext, TimeProvider timeProvider, OverlapPolicy overlapPolicy)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
        _overlapPolicy = overlapPolicy;
    }

    public async Task<Result> HandleAsync(EditPendingRequestCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var request = _dbContext.VacationRequests
                .SingleOrDefault(candidate => candidate.Id == command.RequestId && candidate.OwnerId == command.UserId);

            if (request is null)
            {
                return Result.Failure(new Error(ErrorCodes.NotFound, "Solicitud no encontrada."));
            }

            if (request.Status != RequestStatus.Pending)
            {
                return Result.Failure(Error.Forbidden("Solo se pueden editar solicitudes pendientes."));
            }

            if (!request.RowVersion.SequenceEqual(command.RowVersion))
            {
                return Result.Failure(Error.Conflict("La solicitud fue modificada por otro proceso."));
            }

            var timestamp = _timeProvider.GetUtcNow();
            var dateRange = BuildDateRange(command);
            WorkingDaysCalculator.EnsureStartsAfterBusinessDate(dateRange, _timeProvider);
            var workingDays = WorkingDaysCalculator.Count(dateRange);

            var existingRequests = _dbContext.VacationRequests
                .Where(candidate => candidate.OwnerId == command.UserId && candidate.Id != command.RequestId)
                .ToList();

            if (_overlapPolicy.HasBlockingOverlap(existingRequests, dateRange))
            {
                return Result.Failure(Error.Validation("La solicitud se traslapa con otra solicitud pendiente o aprobada."));
            }

            var balance = _dbContext.VacationBalances
                .SingleOrDefault(candidate => candidate.UserId == command.UserId);

            if (balance is null)
            {
                return Result.Failure(new Error(ErrorCodes.NotFound, "Saldo no encontrado."));
            }

            var previousWorkingDays = request.WorkingDays;
            request.EditPending(dateRange, workingDays, command.Reason, timestamp);

            var delta = workingDays.Value - previousWorkingDays;
            if (delta > 0)
            {
                var movement = balance.Reserve(request.Id, delta, command.UserId, timestamp);
                _dbContext.AddBalanceMovement(movement);
            }
            else if (delta < 0)
            {
                var movement = balance.Release(request.Id, Math.Abs(delta), command.UserId, timestamp);
                _dbContext.AddBalanceMovement(movement);
            }

            _dbContext.AddAuditRecord(AuditRecord.Create(
                command.UserId,
                "User",
                "EditPending",
                nameof(VacationRequest),
                request.Id,
                "Success",
                Guid.NewGuid(),
                request.Id,
                $"{{\"StartDate\":\"{request.StartDate:yyyy-MM-dd}\",\"EndDate\":\"{request.EndDate:yyyy-MM-dd}\",\"WorkingDays\":{request.WorkingDays}}}",
                timestamp));

            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (ArgumentException exception)
        {
            return Result.Failure(Error.Validation(exception.Message));
        }
        catch (InvalidOperationException exception)
        {
            return Result.Failure(Error.Validation(exception.Message));
        }
    }

    private static DateRange BuildDateRange(EditPendingRequestCommand command)
    {
        if (command.InputMode == Models.RequestInputMode.StartPlusDays)
        {
            var workingDays = WorkingDayCount.From(command.WorkingDays ?? 0);
            return WorkingDaysCalculator.FromStartAndWorkingDays(command.StartDate, workingDays);
        }

        return new DateRange(command.StartDate, command.EndDate ?? command.StartDate);
    }
}
