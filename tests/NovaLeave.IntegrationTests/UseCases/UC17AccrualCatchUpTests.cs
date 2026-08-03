using Microsoft.Extensions.DependencyInjection;
using NovaLeave.Application.System.ExecuteMonthlyAccrual;
using NovaLeave.Domain.Enums;
using NovaLeave.Infrastructure.Persistence;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.UseCases;

public sealed class UC17AccrualCatchUpTests
{
    [Fact]
    public async Task CatchUp_Processes_Each_Missed_Eligible_Period_Once()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(
            factory,
            "user-1",
            "user1@example.test",
            accruedDays: 0,
            employmentStartDate: new DateOnly(2026, 1, 1));

        using var scope = factory.Services.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ExecuteMonthlyAccrualHandler>();

        var result = await handler.HandleAsync(new ExecuteMonthlyAccrualCommand(new DateOnly(2026, 5, 10)), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        var balance = db.VacationBalances.Single(candidate => candidate.UserId == "user-1");
        Assert.Equal(4, balance.AccruedDays);
        Assert.Equal(
            [
                new DateOnly(2026, 1, 1),
                new DateOnly(2026, 2, 1),
                new DateOnly(2026, 3, 1),
                new DateOnly(2026, 4, 1)
            ],
            db.BalanceMovements
                .Where(movement => movement.BalanceId == balance.Id && movement.Type == MovementType.Accrual)
                .OrderBy(movement => movement.AccrualPeriod)
                .Select(movement => movement.AccrualPeriod!.Value)
                .ToList());
    }

    [Fact]
    public async Task CatchUp_Skips_Periods_Already_Accrued_By_Earlier_Runs()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(
            factory,
            "user-1",
            "user1@example.test",
            accruedDays: 0,
            employmentStartDate: new DateOnly(2026, 1, 1));

        using var scope = factory.Services.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ExecuteMonthlyAccrualHandler>();

        var first = await handler.HandleAsync(new ExecuteMonthlyAccrualCommand(new DateOnly(2026, 3, 1)), CancellationToken.None);
        var catchUp = await handler.HandleAsync(new ExecuteMonthlyAccrualCommand(new DateOnly(2026, 5, 1)), CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.True(catchUp.IsSuccess);
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        var balance = db.VacationBalances.Single(candidate => candidate.UserId == "user-1");
        Assert.Equal(4, balance.AccruedDays);
        Assert.Equal(4, db.BalanceMovements.Count(movement => movement.BalanceId == balance.Id && movement.Type == MovementType.Accrual));
    }
}
