using System.Net;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.UseCases;

public sealed class UC10ApproverDetailTests
{
    [Fact]
    public async Task Detail_Shows_Server_Derived_Projected_Balance_And_RowVersion()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await ApproverTestData.SeedUserAndApproverAsync(factory);
        var client = factory.CreateClient();
        var requestId = await ApproverTestData.CreatePendingRequestAsync(factory, client);

        var response = await client.SendAsync(ApproverTestData.ApproverGet($"/aprobaciones/{requestId}"));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Detalle para resolucion", html);
        Assert.Contains("Saldo proyectado", html);
        Assert.Contains("RowVersion", html);
        Assert.DoesNotContain("Vacaciones familiares", html);
    }

    [Fact]
    public async Task Detail_Denies_Self_Resolution_And_Disabled_Approver()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await ApproverTestData.SeedUserAndApproverAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "combo-1", "combo@example.test", 10, roles: "User,Approver", canResolveRequests: true);
        var client = factory.CreateClient();
        var requestId = await ApproverTestData.CreatePendingRequestAsync(factory, client, "combo-1");

        var selfResponse = await client.SendAsync(ApproverTestData.ApproverGet($"/aprobaciones/{requestId}", "combo-1"));
        var disabledResponse = await client.SendAsync(ApproverTestData.ApproverGet($"/aprobaciones/{requestId}", canResolve: false));

        Assert.Equal(HttpStatusCode.NotFound, selfResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, disabledResponse.StatusCode);
    }
}
