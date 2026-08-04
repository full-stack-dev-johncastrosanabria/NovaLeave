using System.Net;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.UseCases;

public sealed class UC08PersonalCalendarTests
{
    [Fact]
    public async Task Personal_Calendar_Is_Authorized_User_Scope()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await ApproverTestData.SeedUserAndApproverAsync(factory);
        var client = factory.CreateClient();
        var requestId = await ApproverTestData.CreatePendingRequestAsync(factory, client);
        var rowVersion = ApproverTestData.RowVersionFor(factory, requestId);
        await client.SendAsync(ApproverTestData.ApproverPost(
            $"/aprobaciones/{requestId}/aprobar",
            ApproverTestData.Form(("RowVersion", rowVersion))));

        var response = await client.SendAsync(IntegrationTestDatabase.AuthenticatedGet("/calendario"));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Mi calendario", html);
        Assert.Contains("Calendario de vacaciones", html);
        Assert.Contains("user1@example.test", html);
    }
}
