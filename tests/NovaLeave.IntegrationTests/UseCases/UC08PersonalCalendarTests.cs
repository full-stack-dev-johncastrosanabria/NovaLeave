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
}
