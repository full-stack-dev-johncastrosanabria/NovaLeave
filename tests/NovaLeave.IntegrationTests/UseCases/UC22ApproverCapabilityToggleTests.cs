using System.Net;
using Microsoft.Extensions.DependencyInjection;
using NovaLeave.Infrastructure.Persistence;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.UseCases;

public sealed class UC22ApproverCapabilityToggleTests
{
    [Fact]
    public async Task Active_HR_Can_Toggle_CanResolveRequests_With_Reason_Confirmation_RowVersion_And_Audit()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await SeedAsync(factory);
        var client = factory.CreateClient();
        var rowVersion = RowVersionFor(factory, "approver-1");

        var response = await client.SendAsync(HRPost("/rrhh/aprobadores/approver-1/capacidad",
            ("Enable", "false"),
            ("Reason", "Rotacion temporal de guardia de aprobaciones."),
            ("Confirmed", "true"),
            ("RowVersion", rowVersion)));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        var user = db.Users.Single(user => user.Id == "approver-1");
        Assert.False(user.CanResolveRequests);

        var audit = db.AuditRecords.Single(record => record.Action == "ToggleCapability" && record.EntityType == "ApplicationUser");
        Assert.Equal("hr-1", audit.ActorId);
        Assert.Equal("HR", audit.ActorRole);
        Assert.Equal("Success", audit.Result);
        Assert.Contains("Rotacion temporal", audit.Data);
        Assert.Contains("\"Before\":true", audit.Data);
        Assert.Contains("\"After\":false", audit.Data);
    }

    [Theory]
    [InlineData("", "true", HttpStatusCode.BadRequest)]
    [InlineData("Cambio temporal documentado.", "false", HttpStatusCode.BadRequest)]
    public async Task Toggle_Rejects_Missing_Reason_Or_Confirmation_And_Audits_Failure(string reason, string confirmed, HttpStatusCode expected)
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await SeedAsync(factory);
        var client = factory.CreateClient();
        var rowVersion = RowVersionFor(factory, "approver-1");

        var response = await client.SendAsync(HRPost("/rrhh/aprobadores/approver-1/capacidad",
            ("Enable", "false"),
            ("Reason", reason),
            ("Confirmed", confirmed),
            ("RowVersion", rowVersion)));

        Assert.Equal(expected, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        Assert.True(db.Users.Single(user => user.Id == "approver-1").CanResolveRequests);
        Assert.Contains(db.AuditRecords, record => record.Action == "ToggleCapabilityFailed" && record.ActorId == "hr-1");
    }

    [Fact]
    public async Task Toggle_Rejects_Stale_RowVersion_And_Does_Not_Mutate()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await SeedAsync(factory);
        var client = factory.CreateClient();

        var response = await client.SendAsync(HRPost("/rrhh/aprobadores/approver-1/capacidad",
            ("Enable", "false"),
            ("Reason", "Cambio temporal documentado."),
            ("Confirmed", "true"),
            ("RowVersion", Convert.ToBase64String([1, 2, 3]))));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        Assert.True(db.Users.Single(user => user.Id == "approver-1").CanResolveRequests);
        Assert.Contains(db.AuditRecords, record => record.Action == "ToggleCapabilityFailed" && record.Result == "Conflict");
    }

    [Fact]
    public async Task Toggle_Takes_Effect_Immediately_For_Approver_Authorization()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await ApproverTestData.SeedUserAndApproverAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "hr-1", "hr@example.test", 0, roles: "HR");
        var client = factory.CreateClient();
        var rowVersion = RowVersionFor(factory, "approver-1");

        var toggle = await client.SendAsync(HRPost("/rrhh/aprobadores/approver-1/capacidad",
            ("Enable", "false"),
            ("Reason", "Rotacion temporal de guardia de aprobaciones."),
            ("Confirmed", "true"),
            ("RowVersion", rowVersion)));
        var approverQueue = await client.SendAsync(ApproverTestData.ApproverGet("/aprobaciones", canResolve: true));

        Assert.Equal(HttpStatusCode.Redirect, toggle.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, approverQueue.StatusCode);
    }

    private static async Task SeedAsync(NovaLeaveWebApplicationFactory factory)
    {
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "hr-1", "hr@example.test", 0, roles: "HR");
        await IntegrationTestDatabase.SeedUserAsync(factory, "approver-1", "approver@example.test", 0, roles: "Approver", canResolveRequests: true);
    }

    private static string RowVersionFor(NovaLeaveWebApplicationFactory factory, string userId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        return Convert.ToBase64String(db.Users.Single(user => user.Id == userId).RowVersion);
    }

    private static HttpRequestMessage HRPost(string path, params (string Key, string Value)[] values)
    {
        return IntegrationTestDatabase.AuthenticatedPost(
            path,
            new FormUrlEncodedContent(values.Select(value => new KeyValuePair<string, string>(value.Key, value.Value))),
            "hr-1",
            "HR");
    }
}
