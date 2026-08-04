using System.Net;
using Microsoft.Extensions.DependencyInjection;
using NovaLeave.Domain.Enums;
using NovaLeave.Infrastructure.Persistence;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.UseCases;

public sealed class UC04CreateVacationRequestTests
{
    [Fact]
    public async Task Create_Form_Shows_Balance_Summary_And_Derived_Field_Hooks()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "user-1", "user1@example.test", 10);
        var client = factory.CreateClient();

        var response = await client.SendAsync(IntegrationTestDatabase.AuthenticatedGet("/mis-solicitudes/crear"));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Acumulado total", html);
        Assert.Contains("Pendientes", html);
        Assert.Contains("Disponible actual", html);
        Assert.Contains("Saldo posterior estimado", html);
        Assert.Contains("data-available-days=\"10\"", html);
        Assert.Contains("data-derived-end-date", html);
        Assert.DoesNotContain("Cómo se calcula", html);
    }

    [Fact]
    public async Task DateRange_Create_Reserves_Balance_And_Audits_Atomically()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "user-1", "user1@example.test", 10);
        var client = factory.CreateClient();

        var response = await client.SendAsync(IntegrationTestDatabase.AuthenticatedPost("/mis-solicitudes/crear", Form(
            ("InputMode", "dateRange"),
            ("StartDate", "2027-01-04"),
            ("EndDate", "2027-01-06"),
            ("Reason", "Vacaciones familiares de inicio de ano."),
            ("OwnerId", "attacker"),
            ("Status", "Approved"),
            ("WorkingDays", "99"))));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        var request = db.VacationRequests.Single();
        var balance = db.VacationBalances.Single();

        Assert.Equal("user-1", request.OwnerId);
        Assert.Equal(RequestStatus.Pending, request.Status);
        Assert.Equal(3, request.WorkingDays);
        Assert.Equal(3, balance.ReservedDays);
        Assert.Single(db.AuditRecords);
        Assert.Single(db.BalanceMovements.Where(movement => movement.Type == MovementType.Reservation));
    }

    [Fact]
    public async Task StartPlusDays_Create_Derives_EndDate_And_Rejects_Weekend_Only_Range()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "user-1", "user1@example.test", 10);
        var client = factory.CreateClient();

        var created = await client.SendAsync(IntegrationTestDatabase.AuthenticatedPost("/mis-solicitudes/crear", Form(
            ("InputMode", "startPlusDays"),
            ("StartDate", "2027-01-08"),
            ("WorkingDays", "2"),
            ("Reason", "Vacaciones familiares de fin de semana."))));

        Assert.Equal(HttpStatusCode.Redirect, created.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        Assert.Equal(new DateOnly(2027, 1, 11), db.VacationRequests.Single().EndDate);

        var rejected = await client.SendAsync(IntegrationTestDatabase.AuthenticatedPost("/mis-solicitudes/crear", Form(
            ("InputMode", "dateRange"),
            ("StartDate", "2027-01-16"),
            ("EndDate", "2027-01-17"),
            ("Reason", "Solo fin de semana no valido."))));

        Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
    }

    [Theory]
    [InlineData("2020-01-01", "2020-01-03")]
    [InlineData("2027-01-10", "2027-01-04")]
    public async Task Create_Rejects_Invalid_Date_Ranges(string startDate, string endDate)
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "user-1", "user1@example.test", 10);
        var client = factory.CreateClient();

        var response = await client.SendAsync(IntegrationTestDatabase.AuthenticatedPost("/mis-solicitudes/crear", Form(
            ("InputMode", "dateRange"),
            ("StartDate", startDate),
            ("EndDate", endDate),
            ("Reason", "Vacaciones familiares invalidas."))));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static FormUrlEncodedContent Form(params (string Key, string Value)[] values)
    {
        return new FormUrlEncodedContent(values.Select(value => new KeyValuePair<string, string>(value.Key, value.Value)));
    }
}
