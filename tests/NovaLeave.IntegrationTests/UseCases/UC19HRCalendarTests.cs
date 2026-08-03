using System.Net;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.UseCases;

public sealed class UC19HRCalendarTests
{
    [Fact]
    public async Task HR_Calendar_Uses_Dedicated_Route_And_Shows_Organization_Scope()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await ApproverTestData.SeedUserAndApproverAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "user-2", "user2@example.test", 8);
        await IntegrationTestDatabase.SeedUserAsync(factory, "hr-1", "hr@example.test", 0, roles: "HR");
        var client = factory.CreateClient();
        var firstRequestId = await ApproverTestData.CreatePendingRequestAsync(factory, client, "user-1");
        await client.SendAsync(IntegrationTestDatabase.AuthenticatedPost("/mis-solicitudes/crear", ApproverTestData.Form(
            ("InputMode", "dateRange"),
            ("StartDate", "2027-02-01"),
            ("EndDate", "2027-02-02"),
            ("Reason", "Descanso planificado por cierre de proyecto.")), "user-2"));

        var response = await client.SendAsync(HRGet("/rrhh/calendario"));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Calendario RRHH", html);
        Assert.Contains("user1@example.test", html);
        Assert.Contains("user2@example.test", html);
        Assert.Contains("Pending", html);
        Assert.Contains(firstRequestId.ToString(), html);
        Assert.Contains($"/rrhh/solicitudes/{firstRequestId}", html);
        Assert.DoesNotContain("/aprobaciones/", html);
    }

    [Fact]
    public async Task HR_Is_Denied_Shared_Calendar_Route()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "hr-1", "hr@example.test", 0, roles: "HR");
        var client = factory.CreateClient();

        var response = await client.SendAsync(HRGet("/calendario"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static HttpRequestMessage HRGet(string path)
    {
        return IntegrationTestDatabase.AuthenticatedGet(path, "hr-1", "HR");
    }
}
