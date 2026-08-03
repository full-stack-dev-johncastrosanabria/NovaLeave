using System.Net;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.Observability;

public sealed class HealthCheckTests
{
    [Fact]
    public async Task Liveness_Endpoint_Returns_Secret_Free_Healthy_Response()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health/live");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Healthy", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ConnectionStrings", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Password", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Readiness_Endpoint_Checks_Database_Connectivity_Without_Secrets()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(factory);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health/ready");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("database", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Server=", content, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Password", content, StringComparison.OrdinalIgnoreCase);
    }
}
