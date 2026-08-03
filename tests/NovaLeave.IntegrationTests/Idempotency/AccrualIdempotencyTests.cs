using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NovaLeave.Application.System.ExecuteMonthlyAccrual;
using NovaLeave.Domain.Enums;
using NovaLeave.Infrastructure.Persistence;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.Idempotency;

public sealed class AccrualIdempotencyTests
{
    [Fact]
    public async Task Repeated_Accrual_Run_Applies_Each_User_Period_Exactly_Once()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(
            factory,
            "user-1",
            "user1@example.test",
            accruedDays: 0,
            employmentStartDate: new DateOnly(2026, 3, 1));

        using var scope = factory.Services.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ExecuteMonthlyAccrualHandler>();

        var first = await handler.HandleAsync(new ExecuteMonthlyAccrualCommand(new DateOnly(2026, 4, 1)), CancellationToken.None);
        var second = await handler.HandleAsync(new ExecuteMonthlyAccrualCommand(new DateOnly(2026, 4, 1)), CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        var balance = db.VacationBalances.Single(candidate => candidate.UserId == "user-1");
        Assert.Equal(1, balance.AccruedDays);
        Assert.Single(db.BalanceMovements.Where(movement =>
            movement.BalanceId == balance.Id &&
            movement.Type == MovementType.Accrual &&
            movement.AccrualPeriod == new DateOnly(2026, 3, 1)));
    }

    [Fact]
    public async Task Database_Enforces_Unique_User_Accrual_Period_Index()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(
            factory,
            "user-1",
            "user1@example.test",
            accruedDays: 0,
            employmentStartDate: new DateOnly(2026, 3, 1));

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        var balance = db.VacationBalances.Single(candidate => candidate.UserId == "user-1");
        db.BalanceMovements.Add(NovaLeave.Domain.Entities.BalanceMovement.Create(
            balance.Id,
            null,
            MovementType.Accrual,
            1,
            "System",
            new DateOnly(2026, 3, 1),
            DateTimeOffset.UtcNow));
        db.BalanceMovements.Add(NovaLeave.Domain.Entities.BalanceMovement.Create(
            balance.Id,
            null,
            MovementType.Accrual,
            1,
            "System",
            new DateOnly(2026, 3, 1),
            DateTimeOffset.UtcNow));

        await Assert.ThrowsAnyAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }
}
