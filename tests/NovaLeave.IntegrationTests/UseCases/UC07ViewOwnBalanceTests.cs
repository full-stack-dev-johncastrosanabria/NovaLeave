using System.Net;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.UseCases;

public sealed class UC07ViewOwnBalanceTests
{
    [Fact]
    public async Task Own_Balance_Page_Shows_Authoritative_Totals_And_Movements()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "user-1", "user1@example.test", 10);
        var client = factory.CreateClient();
        await client.SendAsync(IntegrationTestDatabase.AuthenticatedPost("/mis-solicitudes/crear", Form(("InputMode", "dateRange"), ("StartDate", "2027-01-04"), ("EndDate", "2027-01-06"), ("Reason", "Vacaciones familiares de inicio de ano."))));

        var response = await client.SendAsync(IntegrationTestDatabase.AuthenticatedGet("/saldo"));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Acumulado", html);
        Assert.Contains("Pendientes", html);
        Assert.Contains("Disponible", html);
        Assert.Contains("Reserva", html);
    }

    private static FormUrlEncodedContent Form(params (string Key, string Value)[] values)
    {
        return new FormUrlEncodedContent(values.Select(value => new KeyValuePair<string, string>(value.Key, value.Value)));
    }
}
