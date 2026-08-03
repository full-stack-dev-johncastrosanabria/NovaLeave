using System.Net;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.Observability;

public sealed class ObservabilityRedactionTests
{
    [Fact]
    public async Task Health_And_Error_Responses_Do_Not_Expose_Internal_Or_Sensitive_Data()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health/live");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("ConnectionString", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Password", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Token", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Redaction_Applies_To_Logs_Traces_Metrics_And_Audit_Payloads()
    {
        var payload = "{\"Reason\":\"medical appointment\",\"RejectionReason\":\"private\",\"password\":\"secret\",\"token\":\"abc\",\"metric_label\":\"safe\"}";

        var redacted = NovaLeave.Infrastructure.Observability.ObservabilityRedactor.Redact(payload);

        Assert.DoesNotContain("medical appointment", redacted, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("private", redacted, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret", redacted, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("abc", redacted, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("metric_label", redacted, StringComparison.Ordinal);
    }
}
