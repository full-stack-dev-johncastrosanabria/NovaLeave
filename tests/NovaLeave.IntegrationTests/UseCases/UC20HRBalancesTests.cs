using System.Net;
using Microsoft.Extensions.DependencyInjection;
using NovaLeave.Domain.Entities;
using NovaLeave.Infrastructure.Identity;
using NovaLeave.Infrastructure.Persistence;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.UseCases;

public sealed class UC20HRBalancesTests
{
    [Fact]
    public async Task HR_Balances_List_Shows_All_User_Totals_Read_Only()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await SeedBalancesAsync(factory);
        var client = factory.CreateClient();

        var response = await client.SendAsync(HRGet("/rrhh/saldos"));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Saldos RRHH", html);
        Assert.Contains("user1@example.test", html);
        Assert.Contains("user2@example.test", html);
        Assert.Contains("Acumulado total", html);
        Assert.Contains("Pendientes", html);
        Assert.Contains("Días gozados", html);
        Assert.Contains("Disponible", html);
        Assert.DoesNotContain("Editar saldo", html);
        Assert.DoesNotContain("Ajustar", html);
    }

    [Fact]
    public async Task HR_Balance_Detail_Shows_Movements_Read_Only()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await SeedBalancesAsync(factory);
        var client = factory.CreateClient();

        var response = await client.SendAsync(HRGet("/rrhh/saldos/user-1"));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Movimientos RRHH", html);
        Assert.Contains("Accrual", html);
        Assert.DoesNotContain("Editar saldo", html);
        Assert.DoesNotContain("Ajustar", html);
    }

    [Fact]
    public async Task HR_Balances_List_Uses_Approved_Fifty_Row_Pagination()
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
                db.VacationBalances.Add(VacationBalance.Create($"bulk-user-{index:00}", DateTimeOffset.UtcNow));
            }

            await db.SaveChangesAsync();
        }

        var client = factory.CreateClient();
        var response = await client.SendAsync(HRGet("/rrhh/saldos?page=2&pageSize=50"));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Página 2 de 2", html);
        Assert.Contains("bulk-user-51", html);
    }

    private static async Task SeedBalancesAsync(NovaLeaveWebApplicationFactory factory)
    {
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "user-1", "user1@example.test", 10);
        await IntegrationTestDatabase.SeedUserAsync(factory, "user-2", "user2@example.test", 8);
        await IntegrationTestDatabase.SeedUserAsync(factory, "hr-1", "hr@example.test", 0, roles: "HR");
    }

    private static HttpRequestMessage HRGet(string path)
    {
        return IntegrationTestDatabase.AuthenticatedGet(path, "hr-1", "HR");
    }
}
