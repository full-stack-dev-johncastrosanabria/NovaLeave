using NovaLeave.Application.Common.Interfaces;
using NovaLeave.Domain.Enums;

namespace NovaLeave.Application.Observability;

public sealed class BusinessInvariantMonitor : IBusinessInvariantMonitor
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IOperationalTelemetry _telemetry;

    public BusinessInvariantMonitor(IApplicationDbContext dbContext, IOperationalTelemetry telemetry)
    {
        _dbContext = dbContext;
        _telemetry = telemetry;
    }

    public Task<BusinessInvariantResult> EvaluateAsync(CancellationToken cancellationToken)
    {
        var violations = new List<string>();

        foreach (var balance in _dbContext.VacationBalances.ToList())
        {
            if (balance.AccruedDays < 0 || balance.ReservedDays < 0 || balance.DeductedDays < 0 || balance.AvailableDays < 0)
            {
                violations.Add($"balance.non_negative:{balance.UserId}");
            }

            var pendingDays = _dbContext.VacationRequests
                .Where(request => request.OwnerId == balance.UserId && request.Status == RequestStatus.Pending)
                .Sum(request => request.WorkingDays);
            if (balance.ReservedDays != pendingDays)
            {
                violations.Add($"balance.reservation_consistency:{balance.UserId}");
            }

            var approvedDays = _dbContext.VacationRequests
                .Where(request => request.OwnerId == balance.UserId && request.Status == RequestStatus.Approved)
                .Sum(request => request.WorkingDays);
            if (balance.DeductedDays < approvedDays)
            {
                violations.Add($"balance.deduction_consistency:{balance.UserId}");
            }
        }

        var duplicateMovements = _dbContext.BalanceMovements
            .Where(movement => movement.RequestId != null)
            .GroupBy(movement => new { movement.RequestId, movement.Type })
            .Where(group => group.Count() > 1)
            .Select(group => $"movement.duplicate:{group.Key.RequestId}:{group.Key.Type}")
            .ToList();
        violations.AddRange(duplicateMovements);

        foreach (var violation in violations)
        {
            _telemetry.RecordInvariantViolation(violation);
        }

        return Task.FromResult(violations.Count == 0
            ? BusinessInvariantResult.Healthy()
            : BusinessInvariantResult.Unhealthy(violations));
    }
}
