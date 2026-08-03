using System.Net;
using Microsoft.Extensions.DependencyInjection;
using NovaLeave.Infrastructure.Persistence;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.UseCases;

public sealed class UC05EditPendingRequestTests
{
    [Fact]
    public async Task Owner_Can_Edit_Pending_Request_With_Revalidation_And_RowVersion()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "user-1", "user1@example.test", 10);
        var client = factory.CreateClient();
        await client.SendAsync(IntegrationTestDatabase.AuthenticatedPost("/mis-solicitudes/crear", Form(("InputMode", "dateRange"), ("StartDate", "2027-01-04"), ("EndDate", "2027-01-06"), ("Reason", "Vacaciones familiares de inicio de ano."))));

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        var request = db.VacationRequests.Single();
        var rowVersion = Convert.ToBase64String(request.RowVersion);

        var response = await client.SendAsync(IntegrationTestDatabase.AuthenticatedPost($"/mis-solicitudes/{request.Id}/editar", Form(
            ("InputMode", "dateRange"),
            ("StartDate", "2027-01-04"),
            ("EndDate", "2027-01-08"),
            ("Reason", "Vacaciones familiares extendidas."),
            ("RowVersion", rowVersion))));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        db.ChangeTracker.Clear();
        Assert.Equal(5, db.VacationRequests.Single().WorkingDays);
        Assert.Equal(5, db.VacationBalances.Single().ReservedDays);
        Assert.Equal(2, db.AuditRecords.Count());
    }

    [Fact]
    public async Task Stale_RowVersion_Edit_Returns_Conflict()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "user-1", "user1@example.test", 10);
        var client = factory.CreateClient();
        await client.SendAsync(IntegrationTestDatabase.AuthenticatedPost("/mis-solicitudes/crear", Form(("InputMode", "dateRange"), ("StartDate", "2027-01-04"), ("EndDate", "2027-01-06"), ("Reason", "Vacaciones familiares de inicio de ano."))));

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        var request = db.VacationRequests.Single();

        var response = await client.SendAsync(IntegrationTestDatabase.AuthenticatedPost($"/mis-solicitudes/{request.Id}/editar", Form(
            ("InputMode", "dateRange"),
            ("StartDate", "2027-01-04"),
            ("EndDate", "2027-01-08"),
            ("Reason", "Vacaciones familiares extendidas."),
            ("RowVersion", Convert.ToBase64String([1, 2, 3])))));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("La informacion cambio", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Vacaciones familiares extendidas.", html);
    }

    private static FormUrlEncodedContent Form(params (string Key, string Value)[] values)
    {
        return new FormUrlEncodedContent(values.Select(value => new KeyValuePair<string, string>(value.Key, value.Value)));
    }
}
