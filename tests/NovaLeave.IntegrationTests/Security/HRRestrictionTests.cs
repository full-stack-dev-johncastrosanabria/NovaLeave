using System.Net;
using NovaLeave.IntegrationTests.Support;
using NovaLeave.IntegrationTests.UseCases;

namespace NovaLeave.IntegrationTests.Security;

public sealed class HRRestrictionTests
{
    [Fact]
    public async Task HR_Cannot_Approve_Reject_Or_Deactivate_Requests()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await ApproverTestData.SeedUserAndApproverAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "hr-1", "hr@example.test", 0, roles: "HR");
        var client = factory.CreateClient();
        var requestId = await ApproverTestData.CreatePendingRequestAsync(factory, client, "user-1");
        var rowVersion = ApproverTestData.RowVersionFor(factory, requestId);

        var approve = await client.SendAsync(HRPost($"/aprobaciones/{requestId}/aprobar", ("RowVersion", rowVersion)));
        var reject = await client.SendAsync(HRPost($"/aprobaciones/{requestId}/rechazar", ("RowVersion", rowVersion), ("RejectionReason", "Solicitud no autorizada por RRHH.")));
        var deactivate = await client.SendAsync(HRPost($"/aprobaciones/{requestId}/desactivar", ("RowVersion", rowVersion)));

        Assert.Equal(HttpStatusCode.Forbidden, approve.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, reject.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, deactivate.StatusCode);
    }

    [Theory]
    [InlineData("/rrhh/saldos/user-1")]
    [InlineData("/rrhh/roles/user-1")]
    [InlineData("/rrhh/usuarios/user-1/roles")]
    public async Task HR_Forged_Mutation_Routes_Are_Denied(string path)
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "user-1", "user1@example.test", 10);
        await IntegrationTestDatabase.SeedUserAsync(factory, "hr-1", "hr@example.test", 0, roles: "HR");
        var client = factory.CreateClient();

        var response = await client.SendAsync(HRPost(path, ("Value", "forged")));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("User", true)]
    [InlineData("Approver", true)]
    [InlineData("HR", false)]
    public async Task HR_Read_Routes_Deny_Forced_Browsing_By_Non_HR_Or_Inactive_HR(string roles, bool isActive)
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "user-1", "user1@example.test", 10, roles: "User");
        var client = factory.CreateClient();

        var response = await client.SendAsync(IntegrationTestDatabase.AuthenticatedGet("/rrhh/solicitudes", "user-1", roles, isActive));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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
