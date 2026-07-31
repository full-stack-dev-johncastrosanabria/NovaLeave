using NovaLeave.Application.Common.Errors;
using NovaLeave.Application.Common.Interfaces;
using NovaLeave.Application.Common.Results;
using NovaLeave.Domain.Entities;
using NovaLeave.Domain.Services;
using NovaLeave.Domain.ValueObjects;

namespace NovaLeave.Application.Requests.CreateVacationRequest;

public sealed class CreateVacationRequestHandler
{
    private readonly IApplicationDbContext _dbContext;
    private readonly TimeProvider _timeProvider;
    private readonly OverlapPolicy _overlapPolicy;

    public CreateVacationRequestHandler(IApplicationDbContext dbContext, TimeProvider timeProvider, OverlapPolicy overlapPolicy)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
        _overlapPolicy = overlapPolicy;
    }

    public async Task<Result<Guid>> HandleAsync(CreateVacationRequestCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var timestamp = _timeProvider.GetUtcNow();
            var dateRange = BuildDateRange(command);
            WorkingDaysCalculator.EnsureStartsAfterBusinessDate(dateRange, _timeProvider);
            var workingDays = WorkingDaysCalculator.Count(dateRange);

            var existingRequests = _dbContext.VacationRequests
                .Where(request => request.OwnerId == command.UserId)
                .ToList();

            if (_overlapPolicy.HasBlockingOverlap(existingRequests, dateRange))
            {
                return Result<Guid>.Failure(Error.Validation("La solicitud se traslapa con otra solicitud pendiente o aprobada."));
            }

            var balance = _dbContext.VacationBalances
                .SingleOrDefault(candidate => candidate.UserId == command.UserId);

            if (balance is null)
            {
                return Result<Guid>.Failure(new Error(ErrorCodes.NotFound, "Saldo no encontrado."));
            }

            var request = VacationRequest.Create(command.UserId, dateRange, workingDays, command.Reason, timestamp);
            var movement = balance.Reserve(request.Id, workingDays.Value, command.UserId, timestamp);
            _dbContext.AddVacationRequest(request);
            _dbContext.AddBalanceMovement(movement);
            _dbContext.AddAuditRecord(AuditRecord.Create(
                command.UserId,
                "User",
                "Create",
                nameof(VacationRequest),
                request.Id,
                "Success",
                Guid.NewGuid(),
                request.Id,
                $"{{\"StartDate\":\"{request.StartDate:yyyy-MM-dd}\",\"EndDate\":\"{request.EndDate:yyyy-MM-dd}\",\"WorkingDays\":{request.WorkingDays}}}",
                timestamp));

            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Success(request.Id);
        }
        catch (ArgumentException exception)
        {
            return Result<Guid>.Failure(Error.Validation(exception.Message));
        }
        catch (InvalidOperationException exception)
        {
            return Result<Guid>.Failure(Error.Validation(exception.Message));
        }
    }

    private static DateRange BuildDateRange(CreateVacationRequestCommand command)
    {
        if (command.InputMode == Models.RequestInputMode.StartPlusDays)
        {
            var workingDays = WorkingDayCount.From(command.WorkingDays ?? 0);
            return WorkingDaysCalculator.FromStartAndWorkingDays(command.StartDate, workingDays);
        }

        return new DateRange(command.StartDate, command.EndDate ?? command.StartDate);
    }
}
