using System.Net;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.UseCases;

public sealed class UC14ResolutionHistoryTests
{
    [Fact]
    public async Task Resolution_History_Shows_Approver_Audit_Entries()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await ApproverTestData.SeedUserAndApproverAsync(factory);
        var client = factory.CreateClient();
        var requestId = await ApproverTestData.CreatePendingRequestAsync(factory, client);
        var rowVersion = ApproverTestData.RowVersionFor(factory, requestId);
        await client.SendAsync(ApproverTestData.ApproverPost($"/aprobaciones/{requestId}/aprobar", ApproverTestData.Form(("RowVersion", rowVersion))));

        var response = await client.SendAsync(ApproverTestData.ApproverGet("/aprobaciones/historial"));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Historial de resoluciones", html);
        Assert.Contains("Approve", html);
        Assert.Contains(requestId.ToString(), html);
    }
}
