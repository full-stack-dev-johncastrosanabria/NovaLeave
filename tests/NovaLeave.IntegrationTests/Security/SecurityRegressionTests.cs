using System.Net;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NovaLeave.Domain.Enums;
using NovaLeave.Infrastructure.Persistence;
using NovaLeave.IntegrationTests.Support;
using NovaLeave.IntegrationTests.UseCases;

namespace NovaLeave.IntegrationTests.Security;

public sealed class SecurityRegressionTests
{
    [Fact]
    public async Task Post_Mutations_Require_Antiforgery_When_Real_Mvc_Filter_Is_Enabled()
    {
        await using var factory = new AntiforgeryWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "user-1", "user1@example.test", 10);
        var client = factory.CreateClient();

        var response = await client.SendAsync(IntegrationTestDatabase.AuthenticatedPost(
            "/mis-solicitudes/crear",
            ApproverTestData.Form(
                ("InputMode", "dateRange"),
                ("StartDate", "2027-01-04"),
                ("EndDate", "2027-01-06"),
                ("Reason", "Vacaciones familiares de inicio de ano.")),
            "user-1",
            "User"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task IDOR_And_Forced_Browsing_Do_Not_Disclose_Cross_User_Request_Details()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "user-1", "user1@example.test", 10);
        await IntegrationTestDatabase.SeedUserAsync(factory, "user-2", "user2@example.test", 10);
        var client = factory.CreateClient();
        var requestId = await ApproverTestData.CreatePendingRequestAsync(factory, client, "user-1");

        var detail = await client.SendAsync(IntegrationTestDatabase.AuthenticatedGet($"/mis-solicitudes/{requestId}", "user-2", "User"));
        var edit = await client.SendAsync(IntegrationTestDatabase.AuthenticatedGet($"/mis-solicitudes/{requestId}/editar", "user-2", "User"));
        var mutation = await client.SendAsync(IntegrationTestDatabase.AuthenticatedPost(
            $"/mis-solicitudes/{requestId}/editar",
            ApproverTestData.Form(
                ("InputMode", "dateRange"),
                ("StartDate", "2027-02-01"),
                ("EndDate", "2027-02-03"),
                ("Reason", "Intento de acceso cruzado bloqueado."),
                ("RowVersion", ApproverTestData.RowVersionFor(factory, requestId))),
            "user-2",
            "User"));

        Assert.Equal(HttpStatusCode.NotFound, detail.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, edit.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, mutation.StatusCode);
    }

    [Fact]
    public async Task Overposting_Does_Not_Change_Server_Controlled_Request_Fields()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "user-1", "user1@example.test", 10);
        var client = factory.CreateClient();

        var response = await client.SendAsync(IntegrationTestDatabase.AuthenticatedPost(
            "/mis-solicitudes/crear",
            ApproverTestData.Form(
                ("InputMode", "dateRange"),
                ("StartDate", "2027-01-04"),
                ("EndDate", "2027-01-06"),
                ("Reason", "Vacaciones familiares de inicio de ano."),
                ("OwnerId", "user-2"),
                ("Status", "Approved"),
                ("CanResolveRequests", "true"),
                ("WorkingDays", "99")),
            "user-1",
            "User"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        var request = db.VacationRequests.Single();
        Assert.Equal("user-1", request.OwnerId);
        Assert.Equal(RequestStatus.Pending, request.Status);
        Assert.Equal(3, request.WorkingDays);
    }

    [Fact]
    public void Cookie_Session_Timeout_Uses_Approved_Thirty_Minute_Window()
    {
        using var factory = new NovaLeaveWebApplicationFactory();
        var options = factory.Services.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(IdentityConstants.ApplicationScheme);

        Assert.Equal(TimeSpan.FromMinutes(30), options.ExpireTimeSpan);
        Assert.True(options.SlidingExpiration);
    }
}
