using System.Net;
using Microsoft.Extensions.DependencyInjection;
using NovaLeave.Infrastructure.Persistence;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.UseCases;

public sealed class UC06ViewOwnedRequestDetailTests
{
    [Fact]
    public async Task Owner_Can_View_Detail_And_Other_User_Is_Denied()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "user-1", "user1@example.test", 10);
        await IntegrationTestDatabase.SeedUserAsync(factory, "user-2", "user2@example.test", 10);
        var client = factory.CreateClient();
        await client.SendAsync(IntegrationTestDatabase.AuthenticatedPost("/mis-solicitudes/crear", Form(("InputMode", "dateRange"), ("StartDate", "2027-01-04"), ("EndDate", "2027-01-06"), ("Reason", "Vacaciones familiares de inicio de ano."))));

        using var scope = factory.Services.CreateScope();
        var requestId = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>().VacationRequests.Single().Id;

        var ownerResponse = await client.SendAsync(IntegrationTestDatabase.AuthenticatedGet($"/mis-solicitudes/{requestId}"));
        var deniedResponse = await client.SendAsync(IntegrationTestDatabase.AuthenticatedGet($"/mis-solicitudes/{requestId}", "user-2"));

        Assert.Equal(HttpStatusCode.OK, ownerResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, deniedResponse.StatusCode);
    }

    private static FormUrlEncodedContent Form(params (string Key, string Value)[] values)
    {
        return new FormUrlEncodedContent(values.Select(value => new KeyValuePair<string, string>(value.Key, value.Value)));
    }
}
