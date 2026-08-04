using System.Net;
using Microsoft.Extensions.DependencyInjection;
using NovaLeave.Domain.Entities;
using NovaLeave.Domain.ValueObjects;
using NovaLeave.Infrastructure.Identity;
using NovaLeave.Infrastructure.Persistence;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.UseCases;

public sealed class UC18HRRequestsTests
{
    [Fact]
    public async Task HR_Request_List_Shows_Organization_Wide_Read_Only_Requests()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await SeedOrganizationAsync(factory);
        var client = factory.CreateClient();

        var response = await client.SendAsync(HRGet("/rrhh/solicitudes"));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Solicitudes RRHH", html);
        Assert.Contains("user1@example.test", html);
        Assert.Contains("user2@example.test", html);
        Assert.Contains("Pending", html);
        Assert.DoesNotContain("/aprobar", html);
        Assert.DoesNotContain("/rechazar", html);
        Assert.DoesNotContain("/desactivar", html);
    }

    [Fact]
    public async Task HR_Request_Detail_Shows_Authorized_Sensitive_Reason_And_No_Mutations()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        var requestId = await SeedOrganizationAsync(factory);
        var client = factory.CreateClient();

        var response = await client.SendAsync(HRGet($"/rrhh/solicitudes/{requestId}"));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Detalle RRHH", html);
        Assert.Contains("Vacaciones familiares de inicio de ano.", html);
        Assert.DoesNotContain("/aprobar", html);
        Assert.DoesNotContain("/rechazar", html);
        Assert.DoesNotContain("/desactivar", html);
    }

    [Fact]
    public async Task HR_Request_List_Uses_Approved_Fifty_Row_Pagination()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "hr-1", "hr@example.test", 0, roles: "HR");
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
            for (var index = 1; index <= 51; index++)
            {
                db.Users.Add(new ApplicationUser
                {
                    Id = $"bulk-user-{index:00}",
                    UserName = $"bulk-user-{index:00}@example.test",
                    Email = $"bulk-user-{index:00}@example.test",
                    NormalizedUserName = $"BULK-USER-{index:00}@EXAMPLE.TEST",
                    NormalizedEmail = $"BULK-USER-{index:00}@EXAMPLE.TEST",
                    EmailConfirmed = true,
                    IsActive = true,
                    EmploymentStartDate = new DateOnly(2026, 1, 1)
                });
                db.VacationRequests.Add(VacationRequest.Create(
                    $"bulk-user-{index:00}",
                    new DateRange(new DateOnly(2027, 3, 1), new DateOnly(2027, 3, 1)),
                    WorkingDayCount.From(1),
                    "Vacaciones familiares con planificacion suficiente.",
                    DateTimeOffset.UtcNow.AddMinutes(index)));
            }

            await db.SaveChangesAsync();
        }

        var client = factory.CreateClient();
        var response = await client.SendAsync(HRGet("/rrhh/solicitudes?page=2&pageSize=50"));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Página 2 de 2", html);
        Assert.Contains("bulk-user-01", html);
    }

    private static async Task<Guid> SeedOrganizationAsync(NovaLeaveWebApplicationFactory factory)
    {
        await ApproverTestData.SeedUserAndApproverAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "user-2", "user2@example.test", 8);
        await IntegrationTestDatabase.SeedUserAsync(factory, "hr-1", "hr@example.test", 0, roles: "HR");
        var client = factory.CreateClient();
        var firstRequestId = await ApproverTestData.CreatePendingRequestAsync(factory, client, "user-1");
        await client.SendAsync(IntegrationTestDatabase.AuthenticatedPost("/mis-solicitudes/crear", ApproverTestData.Form(
            ("InputMode", "dateRange"),
            ("StartDate", "2027-02-01"),
            ("EndDate", "2027-02-02"),
            ("Reason", "Descanso planificado por cierre de proyecto.")), "user-2"));

        return firstRequestId;
    }

    private static HttpRequestMessage HRGet(string path)
    {
        return IntegrationTestDatabase.AuthenticatedGet(path, "hr-1", "HR");
    }
}
