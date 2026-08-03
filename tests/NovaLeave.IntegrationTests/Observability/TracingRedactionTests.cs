using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using NovaLeave.Application.Observability;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.Observability;

public sealed class TracingRedactionTests
{
    [Fact]
    public async Task Operational_Tracing_Creates_Correlation_Safe_Redacted_Spans()
    {
        var activities = new List<Activity>();
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == NovaLeaveTelemetry.ActivitySourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = activity => activities.Add(activity)
        };
        ActivitySource.AddActivityListener(listener);

        await using var factory = new NovaLeaveWebApplicationFactory();
        var telemetry = factory.Services.GetRequiredService<IOperationalTelemetry>();
        using (var activity = telemetry.StartOperation("request.create", "correlation-001", "Reason=medical appointment; token=secret"))
        {
            activity?.SetTag("db.system", "mssql");
        }

        var operation = Assert.Single(activities, activity => activity.OperationName == "request.create");
        Assert.Equal("correlation-001", operation.GetTagItem("correlation_id"));
        Assert.DoesNotContain("medical appointment", string.Join(";", operation.Tags.Select(tag => tag.Value)), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret", string.Join(";", operation.Tags.Select(tag => tag.Value)), StringComparison.OrdinalIgnoreCase);
    }
}
