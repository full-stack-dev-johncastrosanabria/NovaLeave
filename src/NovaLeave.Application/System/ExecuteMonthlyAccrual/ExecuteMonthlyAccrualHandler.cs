using NovaLeave.Application.Audit;
using NovaLeave.Application.Common.Interfaces;
using NovaLeave.Application.Common.Results;
using NovaLeave.Application.Observability;
using NovaLeave.Domain.Entities;
using NovaLeave.Domain.Enums;
using NovaLeave.Domain.Services;

namespace NovaLeave.Application.System.ExecuteMonthlyAccrual;

public sealed class ExecuteMonthlyAccrualHandler
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAccrualUserSource _userSource;
    private readonly MonthlyAccrualPolicy _policy;
    private readonly SystemAuditWriter _auditWriter;
    private readonly TimeProvider _timeProvider;
    private readonly IOperationalTelemetry _telemetry;

    public ExecuteMonthlyAccrualHandler(
        IApplicationDbContext dbContext,
        IAccrualUserSource userSource,
        MonthlyAccrualPolicy policy,
        SystemAuditWriter auditWriter,
        TimeProvider timeProvider,
        IOperationalTelemetry telemetry)
    {
        _dbContext = dbContext;
        _userSource = userSource;
        _policy = policy;
        _auditWriter = auditWriter;
        _timeProvider = timeProvider;
        _telemetry = telemetry;
    }

    public async Task<Result> HandleAsync(ExecuteMonthlyAccrualCommand command, CancellationToken cancellationToken)
    {
        var users = await _userSource.GetActiveUsersAsync(cancellationToken);

        foreach (var user in users)
        {
            var balance = _dbContext.VacationBalances.SingleOrDefault(candidate => candidate.UserId == user.UserId);
            if (balance is null)
            {
                balance = VacationBalance.Create(user.UserId, _timeProvider.GetUtcNow());
                _dbContext.AddVacationBalance(balance);
            }

            var eligiblePeriods = _policy.GetEligiblePeriods(user.EmploymentStartDate, command.ReferenceDate);
            foreach (var period in eligiblePeriods)
            {
                var accrualPeriod = MonthlyAccrualPolicy.ToDateOnly(period);
                if (HasAccrued(balance.Id, accrualPeriod))
                {
                    continue;
                }

                try
                {
                    var timestamp = _timeProvider.GetUtcNow();
                    var movement = balance.ApplyAccrual(accrualPeriod, SystemAuditWriter.ActorId, timestamp);
                    _dbContext.AddBalanceMovement(movement);
                    _dbContext.AddAuditRecord(_auditWriter.Accrual(balance.Id, user.UserId, accrualPeriod, timestamp));
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    _telemetry.RecordAccrualExecution(true);
                }
                catch (Exception exception) when (IsDuplicateAccrualOrConcurrency(exception))
                {
                    _telemetry.RecordAccrualExecution(false);
                    _telemetry.RecordConcurrencyConflict("accrual.idempotency");
                    // A parallel job already applied this user-period; accrual remains idempotent.
                }
            }
        }

        return Result.Success();
    }

    private bool HasAccrued(Guid balanceId, DateOnly accrualPeriod)
    {
        return _dbContext.BalanceMovements.Any(movement =>
            movement.BalanceId == balanceId &&
            movement.Type == MovementType.Accrual &&
            movement.AccrualPeriod == accrualPeriod);
    }

    private static bool IsDuplicateAccrualOrConcurrency(Exception exception)
    {
        return exception.GetType().Name is "DbUpdateException" or "DbUpdateConcurrencyException";
    }
}
