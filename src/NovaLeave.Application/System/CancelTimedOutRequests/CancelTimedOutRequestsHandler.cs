using NovaLeave.Application.Audit;
using NovaLeave.Application.Common.Interfaces;
using NovaLeave.Application.Common.Results;
using NovaLeave.Application.Configuration;
using NovaLeave.Domain.Entities;
using NovaLeave.Domain.Enums;

namespace NovaLeave.Application.System.CancelTimedOutRequests;

public sealed class CancelTimedOutRequestsHandler
{
    private readonly IApplicationDbContext _dbContext;
    private readonly NovaLeaveOptions _options;
    private readonly SystemAuditWriter _auditWriter;
    private readonly TimeProvider _timeProvider;

    public CancelTimedOutRequestsHandler(
        IApplicationDbContext dbContext,
        NovaLeaveOptions options,
        SystemAuditWriter auditWriter,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _options = options;
        _auditWriter = auditWriter;
        _timeProvider = timeProvider;
    }

    public async Task<Result> HandleAsync(CancelTimedOutRequestsCommand command, CancellationToken cancellationToken)
    {
        var cutoffDate = command.SystemDate.AddDays(-_options.PendingRequestTimeoutDays);
        var eligibleRequests = _dbContext.VacationRequests
            .Where(request =>
                request.Status == RequestStatus.Pending &&
                DateOnly.FromDateTime(request.CreatedAtUtc) <= cutoffDate)
            .OrderBy(request => request.CreatedAtUtc)
            .ToList();

        foreach (var request in eligibleRequests)
        {
            if (request.Status != RequestStatus.Pending)
            {
                continue;
            }

            try
            {
                var timestamp = _timeProvider.GetUtcNow();
                request.CancelByTimeout(timestamp);
                var balance = _dbContext.VacationBalances.Single(candidate => candidate.UserId == request.OwnerId);
                var movement = balance.Release(request.Id, request.WorkingDays, SystemAuditWriter.ActorId, timestamp);
                _dbContext.AddBalanceMovement(movement);
                _dbContext.AddAuditRecord(_auditWriter.Timeout(request.Id, _options.PendingRequestTimeoutDays, timestamp));
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (InvalidOperationException)
            {
                // A concurrent human transition won the race; timeout remains idempotent.
            }
            catch (Exception exception) when (exception.GetType().Name == "DbUpdateConcurrencyException")
            {
                // A concurrent human transition won the race; timeout remains idempotent.
            }
        }

        return Result.Success();
    }
}
