using System.Net;
using Microsoft.Extensions.DependencyInjection;
using NovaLeave.Infrastructure.Persistence;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.Security;

public sealed class HRCapabilityAuthorizationTests
{
    [Fact]
    public async Task Inactive_HR_Cannot_Access_Capability_Management()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await SeedAsync(factory);
        var client = factory.CreateClient();

        var list = await client.SendAsync(IntegrationTestDatabase.AuthenticatedGet("/rrhh/aprobadores", "hr-1", "HR", isActive: false));
        var toggle = await client.SendAsync(HRPost("/rrhh/aprobadores/approver-1/capacidad", false, isActive: false));

        Assert.Equal(HttpStatusCode.Forbidden, list.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, toggle.StatusCode);
    }

    [Fact]
    public async Task Capability_Toggle_Revalidates_HR_Is_Active_In_Database()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "hr-1", "hr@example.test", 0, isActive: false, roles: "HR");
        await IntegrationTestDatabase.SeedUserAsync(factory, "approver-1", "approver@example.test", 0, roles: "Approver", canResolveRequests: true);
        var client = factory.CreateClient();
        var rowVersion = RowVersionFor(factory, "approver-1");

        var response = await client.SendAsync(IntegrationTestDatabase.AuthenticatedPost(
            "/rrhh/aprobadores/approver-1/capacidad",
            new FormUrlEncodedContent([
                new KeyValuePair<string, string>("Enable", "false"),
                new KeyValuePair<string, string>("Reason", "Cambio temporal documentado."),
                new KeyValuePair<string, string>("Confirmed", "true"),
                new KeyValuePair<string, string>("RowVersion", rowVersion)
            ]),
            "hr-1",
            "HR",
            isActive: true));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        Assert.True(db.Users.Single(user => user.Id == "approver-1").CanResolveRequests);
        Assert.Contains(db.AuditRecords, record => record.Action == "ToggleCapabilityFailed" && record.Result == "Forbidden");
    }

    [Fact]
    public async Task Capability_Toggle_Revalidates_Target_Still_Holds_Approver_Role()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await SeedAsync(factory);
        var client = factory.CreateClient();
        var rowVersion = RowVersionFor(factory, "plain-user");

        var response = await client.SendAsync(IntegrationTestDatabase.AuthenticatedPost(
            "/rrhh/aprobadores/plain-user/capacidad",
            new FormUrlEncodedContent([
                new KeyValuePair<string, string>("Enable", "true"),
                new KeyValuePair<string, string>("Reason", "Alta temporal solicitada por operacion."),
                new KeyValuePair<string, string>("Confirmed", "true"),
                new KeyValuePair<string, string>("RowVersion", rowVersion)
            ]),
            "hr-1",
            "HR"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        Assert.False(db.Users.Single(user => user.Id == "plain-user").CanResolveRequests);
        Assert.Contains(db.AuditRecords, record => record.Action == "ToggleCapabilityFailed" && record.EntityId.ToString() == "00000000-0000-0000-0000-000000000000");
    }

    [Theory]
    [InlineData("/rrhh/aprobadores/plain-user/roles")]
    [InlineData("/rrhh/aprobadores/plain-user/asignar")]
    [InlineData("/rrhh/aprobadores/plain-user/remover")]
    public async Task HR_Cannot_Assign_Or_Remove_Roles_Through_Forged_Capability_Routes(string path)
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await SeedAsync(factory);
        var client = factory.CreateClient();

        var response = await client.SendAsync(HRPost(path, true));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static async Task SeedAsync(NovaLeaveWebApplicationFactory factory)
    {
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "hr-1", "hr@example.test", 0, roles: "HR");
        await IntegrationTestDatabase.SeedUserAsync(factory, "approver-1", "approver@example.test", 0, roles: "Approver", canResolveRequests: true);
        await IntegrationTestDatabase.SeedUserAsync(factory, "plain-user", "plain-user@example.test", 0, roles: "User");
    }

    private static string RowVersionFor(NovaLeaveWebApplicationFactory factory, string userId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        return Convert.ToBase64String(db.Users.Single(user => user.Id == userId).RowVersion);
    }

    private static HttpRequestMessage HRPost(string path, bool confirmed, bool isActive = true)
    {
        return IntegrationTestDatabase.AuthenticatedPost(
            path,
            new FormUrlEncodedContent([
                new KeyValuePair<string, string>("Enable", "false"),
                new KeyValuePair<string, string>("Reason", "Cambio temporal documentado."),
                new KeyValuePair<string, string>("Confirmed", confirmed ? "true" : "false"),
                new KeyValuePair<string, string>("RowVersion", Convert.ToBase64String([1, 2, 3]))
            ]),
            "hr-1",
            "HR",
            isActive);
    }
}
