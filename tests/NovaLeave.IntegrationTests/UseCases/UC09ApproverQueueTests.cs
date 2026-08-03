using System.Net;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.UseCases;

public sealed class UC09ApproverQueueTests
{
    [Fact]
    public async Task Eligible_Queue_Shows_Global_Pending_Requests_Excluding_Owned_Requests()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "user-1", "user1@example.test", 10);
        await IntegrationTestDatabase.SeedUserAsync(factory, "approver-1", "approver1@example.test", 10, roles: "User,Approver", canResolveRequests: true);
        var client = factory.CreateClient();

        await ApproverTestData.CreatePendingRequestAsync(factory, client);
        await ApproverTestData.CreatePendingRequestAsync(factory, client, "approver-1");

        var response = await client.SendAsync(ApproverTestData.ApproverGet("/aprobaciones"));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Solicitudes pendientes", html);
        Assert.Contains("user1@example.test", html);
        Assert.DoesNotContain(">user-1<", html);
        Assert.DoesNotContain("approver1@example.test", html);
        Assert.Contains("Disponible actual", html);
        Assert.Contains("Días solicitados", html);
        Assert.Contains("Disponible después de aprobar", html);
        Assert.Contains("data-projected-balance=\"7\"", html);
    }

    [Fact]
    public async Task Queue_Projection_Accounts_For_Other_Reservations_And_Can_Reach_Zero()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "user-1", "user1@example.test", 5);
        await IntegrationTestDatabase.SeedUserAsync(factory, "approver-1", "approver1@example.test", 0, roles: "Approver", canResolveRequests: true);
        var client = factory.CreateClient();

        await ApproverTestData.CreatePendingRequestAsync(factory, client);
        await ApproverTestData.CreatePendingRequestAsync(factory, client, startDate: "2027-01-11", endDate: "2027-01-12");

        var response = await client.SendAsync(ApproverTestData.ApproverGet("/aprobaciones"));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, System.Text.RegularExpressions.Regex.Matches(html, "data-projected-balance=\"0\"").Count);
    }

    [Fact]
    public async Task Queue_Denies_Approver_Without_Capability()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await ApproverTestData.SeedUserAndApproverAsync(factory, canResolve: false);
        var client = factory.CreateClient();

        var response = await client.SendAsync(ApproverTestData.ApproverGet("/aprobaciones", canResolve: false));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
