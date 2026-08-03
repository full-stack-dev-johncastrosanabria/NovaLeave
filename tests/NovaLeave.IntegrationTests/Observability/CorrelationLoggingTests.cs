using System.Net;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.Observability;

public sealed class CorrelationLoggingTests
{
    [Fact]
    public async Task Request_Correlation_Id_Is_Propagated_In_Response_Header()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        var client = factory.CreateClient();
        using var request = IntegrationTestDatabase.AuthenticatedGet("/mis-solicitudes", "user-1", "User");
        request.Headers.Add("X-Correlation-ID", "phase12-correlation-001");
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "user-1", "user1@example.test", 10);

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("X-Correlation-ID", out var values));
        Assert.Equal("phase12-correlation-001", values.Single());
    }

    [Fact]
    public async Task Request_Correlation_Id_Is_Generated_When_Missing()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        var client = factory.CreateClient();
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "user-1", "user1@example.test", 10);

        var response = await client.SendAsync(IntegrationTestDatabase.AuthenticatedGet("/mis-solicitudes", "user-1", "User"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("X-Correlation-ID", out var values));
        Assert.False(string.IsNullOrWhiteSpace(values.Single()));
    }

    [Fact]
    public void Structured_Logging_Redactor_Removes_Sensitive_Free_Text()
    {
        var redacted = NovaLeave.Infrastructure.Observability.ObservabilityRedactor.Redact(
            "Reason=medical appointment; password=secret; token=abc; ConnectionString=Server=tcp; RejectionReason=private");

        Assert.DoesNotContain("medical appointment", redacted, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret", redacted, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("abc", redacted, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Server=tcp", redacted, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("[REDACTED]", redacted, StringComparison.Ordinal);
    }
}
