using System.Net;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.UseCases;

public sealed class UC02SwitchRoleContextTests
{
    [Fact]
    public async Task Single_Context_Identity_Sees_Its_Navigation_Without_The_Role_Switcher()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "user-1", "user1@example.test", 5);
        var client = factory.CreateClient();

        var response = await client.SendAsync(IntegrationTestDatabase.AuthenticatedGet("/mis-solicitudes"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("Mis solicitudes", html);
        Assert.Contains("Mi historial", html);

        // RBFV 4.2 / RBFV-020: the context selector is hidden below two authorized contexts.
        Assert.DoesNotContain("Mis roles", html);

        // A User-only identity is never offered contexts it does not hold.
        Assert.DoesNotContain("/aprobaciones", html);
        Assert.DoesNotContain("/rrhh", html);
    }

    [Fact]
    public async Task Multi_Context_Identity_Sees_The_Role_Switcher_With_Only_Its_Own_Contexts()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "user-1", "user1@example.test", 5, roles: "User,Approver");
        var client = factory.CreateClient();

        var response = await client.SendAsync(
            IntegrationTestDatabase.AuthenticatedGet("/mis-solicitudes", "user-1", "User,Approver", isActive: true, canResolveRequests: true));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("Mis roles", html);
        Assert.Contains("Mi espacio", html);
        Assert.Contains("Aprobaciones", html);

        // RRHH is not held, so it is never offered as a context.
        Assert.DoesNotContain("RRHH", html);
    }

    [Fact]
    public async Task Every_Authenticated_Identity_Can_Sign_Out()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "user-1", "user1@example.test", 5);
        var client = factory.CreateClient();

        var response = await client.SendAsync(IntegrationTestDatabase.AuthenticatedGet("/mis-solicitudes"));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Contains("/Identity/Account/Logout", html);
    }
}
