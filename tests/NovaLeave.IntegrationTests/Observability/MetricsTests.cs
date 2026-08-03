using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using NovaLeave.Application.Observability;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.Observability;

public sealed class MetricsTests
{
    [Fact]
    public async Task Operational_Metrics_Emit_Required_Counters_And_Latency()
    {
        var measurements = new List<string>();
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, meterListener) =>
        {
            if (instrument.Meter.Name == NovaLeaveTelemetry.MeterName)
            {
                meterListener.EnableMeasurementEvents(instrument);
            }
        };
        listener.SetMeasurementEventCallback<long>((instrument, _, _, _) => measurements.Add(instrument.Name));
        listener.SetMeasurementEventCallback<double>((instrument, _, _, _) => measurements.Add(instrument.Name));
        listener.Start();

        await using var factory = new NovaLeaveWebApplicationFactory();
        var telemetry = factory.Services.GetRequiredService<IOperationalTelemetry>();
        telemetry.RecordHttpRequest("GET", "/health/live", 200, TimeSpan.FromMilliseconds(4));
        telemetry.RecordRequestCreated(true);
        telemetry.RecordApproval(true);
        telemetry.RecordRejection(true);
        telemetry.RecordTimeoutCancellation(true);
        telemetry.RecordAccrualExecution(true);
        telemetry.RecordConcurrencyConflict("approval");
        telemetry.RecordInvariantViolation("balance.available_non_negative");
        listener.RecordObservableInstruments();

        Assert.Contains("novaleave.http.requests", measurements);
        Assert.Contains("novaleave.http.request.duration", measurements);
        Assert.Contains("novaleave.vacation_request.created", measurements);
        Assert.Contains("novaleave.vacation_request.approved", measurements);
        Assert.Contains("novaleave.vacation_request.rejected", measurements);
        Assert.Contains("novaleave.job.timeout_cancelled", measurements);
        Assert.Contains("novaleave.job.accrual_executed", measurements);
        Assert.Contains("novaleave.concurrency.conflicts", measurements);
        Assert.Contains("novaleave.invariant.violations", measurements);
    }
}
