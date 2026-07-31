using System.Net;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.UseCases;

public sealed class UC03ViewOwnRequestsTests
{
    [Fact]
    public async Task Own_Request_List_Is_Filtered_To_Authenticated_User()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "user-1", "user1@example.test", 10);
        await IntegrationTestDatabase.SeedUserAsync(factory, "user-2", "user2@example.test", 10);
        var client = factory.CreateClient();

        await client.SendAsync(IntegrationTestDatabase.AuthenticatedPost("/mis-solicitudes/crear", Form(
            ("InputMode", "dateRange"),
            ("StartDate", "2027-01-04"),
            ("EndDate", "2027-01-06"),
            ("Reason", "Vacaciones familiares de inicio de ano."))));

        await client.SendAsync(IntegrationTestDatabase.AuthenticatedPost("/mis-solicitudes/crear", Form(
            ("InputMode", "dateRange"),
            ("StartDate", "2027-02-01"),
            ("EndDate", "2027-02-03"),
            ("Reason", "Solicitud privada de otro usuario.")), "user-2"));

        var response = await client.SendAsync(IntegrationTestDatabase.AuthenticatedGet("/mis-solicitudes"));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("2027-01-04", html);
        Assert.DoesNotContain("2027-02-01", html);
    }

    private static FormUrlEncodedContent Form(params (string Key, string Value)[] values)
    {
        return new FormUrlEncodedContent(values.Select(value => new KeyValuePair<string, string>(value.Key, value.Value)));
    }
}
