using System.Net;
using Microsoft.Extensions.DependencyInjection;
using NovaLeave.Infrastructure.Persistence;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.UseCases;

public sealed class UC22ApproverCapabilityListTests
{
    [Fact]
    public async Task Active_HR_Can_List_Only_Identities_With_Approver_Role()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await SeedAsync(factory);
        var client = factory.CreateClient();

        var response = await client.SendAsync(HRGet("/rrhh/aprobadores"));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Aprobadores RRHH", html);
        Assert.Contains("approver-enabled@example.test", html);
        Assert.Contains("approver-disabled@example.test", html);
        Assert.Contains("Habilitado", html);
        Assert.Contains("Deshabilitado", html);
        Assert.Contains("/rrhh/aprobadores/approver-enabled/capacidad", html);
        Assert.DoesNotContain("plain-user@example.test", html);
        Assert.DoesNotContain("Asignar rol", html);
        Assert.DoesNotContain("Quitar rol", html);
    }

    [Fact]
    public async Task Capability_Form_Shows_Current_State_Expected_RowVersion_Reason_And_Confirmation()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await SeedAsync(factory);
        var client = factory.CreateClient();

        var response = await client.SendAsync(HRGet("/rrhh/aprobadores/approver-enabled/capacidad"));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Capacidad de aprobador", html);
        Assert.Contains("approver-enabled@example.test", html);
        Assert.Contains("name=\"RowVersion\"", html);
        Assert.Contains("name=\"Reason\"", html);
        Assert.Contains("name=\"Confirmed\"", html);
        Assert.DoesNotContain("Asignar rol", html);
        Assert.DoesNotContain("Quitar rol", html);
    }

    private static async Task SeedAsync(NovaLeaveWebApplicationFactory factory)
    {
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "hr-1", "hr@example.test", 0, roles: "HR");
        await IntegrationTestDatabase.SeedUserAsync(factory, "approver-enabled", "approver-enabled@example.test", 0, roles: "Approver", canResolveRequests: true);
        await IntegrationTestDatabase.SeedUserAsync(factory, "approver-disabled", "approver-disabled@example.test", 0, roles: "Approver", canResolveRequests: false);
        await IntegrationTestDatabase.SeedUserAsync(factory, "plain-user", "plain-user@example.test", 0, roles: "User");
    }

    private static HttpRequestMessage HRGet(string path)
    {
        return IntegrationTestDatabase.AuthenticatedGet(path, "hr-1", "HR");
    }
}
