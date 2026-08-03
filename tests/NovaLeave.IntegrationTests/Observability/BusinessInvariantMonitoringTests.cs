using Microsoft.Extensions.DependencyInjection;
using NovaLeave.Application.Observability;
using NovaLeave.Infrastructure.Persistence;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.Observability;

public sealed class BusinessInvariantMonitoringTests
{
    [Fact]
    public async Task Read_Only_Invariant_Monitor_Reports_Healthy_Database_Without_Mutating_Data()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "user-1", "user1@example.test", 10);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        var balanceCount = db.VacationBalances.Count();
        var movementCount = db.BalanceMovements.Count();
        var requestCount = db.VacationRequests.Count();
        var monitor = scope.ServiceProvider.GetRequiredService<IBusinessInvariantMonitor>();

        var result = await monitor.EvaluateAsync(CancellationToken.None);

        Assert.True(result.IsHealthy);
        Assert.Empty(result.Violations);
        Assert.Equal(balanceCount, db.VacationBalances.Count());
        Assert.Equal(movementCount, db.BalanceMovements.Count());
        Assert.Equal(requestCount, db.VacationRequests.Count());
    }
}
