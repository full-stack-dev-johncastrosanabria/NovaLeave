using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NovaLeave.Application.Configuration;
using NovaLeave.Application.System.CancelTimedOutRequests;

namespace NovaLeave.Infrastructure.Scheduling;

public sealed class PendingRequestTimeoutJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PendingRequestTimeoutJob> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly TimeOnly _scheduledTimeUtc;

    public PendingRequestTimeoutJob(
        IServiceScopeFactory scopeFactory,
        IOptions<NovaLeaveOptions> options,
        ILogger<PendingRequestTimeoutJob> logger,
        TimeProvider timeProvider)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _timeProvider = timeProvider;

        if (!NovaLeaveOptions.TryGetDailyUtcTime(options.Value.TimeoutSchedulerCadence, out _scheduledTimeUtc))
        {
            throw new InvalidOperationException("Timeout scheduler cadence must use format 'Daily HH:mm UTC'.");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = GetDelayUntilNextRun(_timeProvider.GetUtcNow());
            await Task.Delay(delay, stoppingToken);
            await RunOnceAsync(stoppingToken);
        }
    }

    internal async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<CancelTimedOutRequestsHandler>();
            var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);
            var result = await handler.HandleAsync(new CancelTimedOutRequestsCommand(today), cancellationToken);
            if (result.IsFailure)
            {
                _logger.LogError("Pending request timeout job failed: {ErrorCode} {ErrorMessage}", result.Error?.Code, result.Error?.Message);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Pending request timeout job failed.");
        }
    }

    private TimeSpan GetDelayUntilNextRun(DateTimeOffset nowUtc)
    {
        var currentDate = DateOnly.FromDateTime(nowUtc.UtcDateTime);
        var nextRun = currentDate.ToDateTime(_scheduledTimeUtc, DateTimeKind.Utc);
        if (nextRun <= nowUtc.UtcDateTime)
        {
            nextRun = currentDate.AddDays(1).ToDateTime(_scheduledTimeUtc, DateTimeKind.Utc);
        }

        return nextRun - nowUtc.UtcDateTime;
    }
}
