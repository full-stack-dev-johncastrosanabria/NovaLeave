using System.Net;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.UseCases;

public sealed class UC15ApproverCalendarTests
{
    [Fact]
    public async Task Approver_Calendar_Shows_Organization_Approved_Events_Anonymized()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await ApproverTestData.SeedUserAndApproverAsync(factory);
        var client = factory.CreateClient();
        var requestId = await ApproverTestData.CreatePendingRequestAsync(factory, client);
        var rowVersion = ApproverTestData.RowVersionFor(factory, requestId);
        await client.SendAsync(ApproverTestData.ApproverPost($"/aprobaciones/{requestId}/aprobar", ApproverTestData.Form(("RowVersion", rowVersion))));

        var response = await client.SendAsync(ApproverTestData.ApproverGet("/calendario"));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Calendario de aprobaciones", html);
        Assert.Contains("Evento aprobado", html);
        Assert.DoesNotContain("user1@example.test", html);
        Assert.DoesNotContain("Vacaciones familiares", html);
    }
}
