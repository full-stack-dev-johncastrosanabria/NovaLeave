using System.Net;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.UseCases;

public sealed class UC08PersonalCalendarTests
{
    [Fact]
    public async Task Personal_Calendar_Is_Authorized_User_Scope()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "user-1", "user1@example.test", 10);
        var client = factory.CreateClient();

        var response = await client.SendAsync(IntegrationTestDatabase.AuthenticatedGet("/calendario"));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Mi calendario", html);
        Assert.Contains("Calendario de vacaciones", html);
    }

    [Fact]
    public async Task Personal_Calendar_Shows_Own_Pending_And_Approved_Requests()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await ApproverTestData.SeedUserAndApproverAsync(factory);
        var client = factory.CreateClient();
        var pendingRequestId = await ApproverTestData.CreatePendingRequestAsync(factory, client, "user-1");

        var response = await client.SendAsync(IntegrationTestDatabase.AuthenticatedGet("/calendario?context=User", "user-1", "User"));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Mis solicitudes", html);
        Assert.Contains("Pendiente", html);
        Assert.Contains($"/mis-solicitudes/{pendingRequestId}", html);
        Assert.DoesNotContain("/aprobaciones/", html);
    }
}
