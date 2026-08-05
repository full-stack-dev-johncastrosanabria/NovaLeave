using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NovaLeave.Application.Configuration;
using NovaLeave.Application.System.ExecuteMonthlyAccrual;
using NovaLeave.Domain.Services;

namespace NovaLeave.Infrastructure.Scheduling;

public sealed class MonthlyAccrualJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MonthlyAccrualJob> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly TimeOnly _scheduledTimeUtc;

    public MonthlyAccrualJob(
        IServiceScopeFactory scopeFactory,
        IOptions<NovaLeaveOptions> options,
        ILogger<MonthlyAccrualJob> logger,
        TimeProvider timeProvider)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _timeProvider = timeProvider;

        if (!NovaLeaveOptions.TryGetDailyUtcTime(options.Value.AccrualSchedulerCadence, out _scheduledTimeUtc))
        {
            throw new InvalidOperationException("Accrual scheduler cadence must use format 'Daily HH:mm UTC'.");
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
            var handler = scope.ServiceProvider.GetRequiredService<ExecuteMonthlyAccrualHandler>();
            var today = CostaRicaTime.GetBusinessDate(_timeProvider.GetUtcNow());
            var result = await handler.HandleAsync(new ExecuteMonthlyAccrualCommand(today), cancellationToken);
            if (result.IsFailure)
            {
                _logger.LogError("Monthly accrual job failed: {ErrorCode} {ErrorMessage}", result.Error?.Code, result.Error?.Message);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Monthly accrual job failed.");
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
