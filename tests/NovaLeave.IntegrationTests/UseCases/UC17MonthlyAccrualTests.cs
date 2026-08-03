using Microsoft.Extensions.DependencyInjection;
using NovaLeave.Application.System.ExecuteMonthlyAccrual;
using NovaLeave.Domain.Enums;
using NovaLeave.Infrastructure.Persistence;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.UseCases;

public sealed class UC17MonthlyAccrualTests
{
    [Fact]
    public async Task Monthly_Accrual_Applies_One_Day_For_Each_Eligible_User_Period_And_Audits_System_Action()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(
            factory,
            "user-mid-month",
            "mid@example.test",
            accruedDays: 0,
            employmentStartDate: new DateOnly(2026, 3, 15));
        await IntegrationTestDatabase.SeedUserAsync(
            factory,
            "user-first-day",
            "first@example.test",
            accruedDays: 0,
            employmentStartDate: new DateOnly(2026, 3, 1));
        await IntegrationTestDatabase.SeedUserAsync(
            factory,
            "approver-only",
            "approver@example.test",
            accruedDays: 0,
            canResolveRequests: true,
            roles: "Approver",
            employmentStartDate: new DateOnly(2026, 3, 1));

        using var scope = factory.Services.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ExecuteMonthlyAccrualHandler>();

        var result = await handler.HandleAsync(new ExecuteMonthlyAccrualCommand(new DateOnly(2026, 5, 1)), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        Assert.Equal(1, db.VacationBalances.Single(balance => balance.UserId == "user-mid-month").AccruedDays);
        Assert.Equal(2, db.VacationBalances.Single(balance => balance.UserId == "user-first-day").AccruedDays);
        Assert.Equal(0, db.VacationBalances.Single(balance => balance.UserId == "approver-only").AccruedDays);
        Assert.Single(db.BalanceMovements.Where(movement =>
            movement.Type == MovementType.Accrual &&
            movement.ActorId == "System" &&
            movement.AccrualPeriod == new DateOnly(2026, 4, 1) &&
            movement.BalanceId == db.VacationBalances.Single(balance => balance.UserId == "user-mid-month").Id));
        Assert.Equal(3, db.AuditRecords.Count(audit => audit.Action == "Accrual" && audit.ActorId == "System"));
    }

    [Fact]
    public async Task Monthly_Accrual_Does_Not_Apply_Proration_For_Month_End_Or_Day_31_Hires()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(
            factory,
            "user-day-31",
            "day31@example.test",
            accruedDays: 0,
            employmentStartDate: new DateOnly(2026, 1, 31));
        await IntegrationTestDatabase.SeedUserAsync(
            factory,
            "user-month-end",
            "monthend@example.test",
            accruedDays: 0,
            employmentStartDate: new DateOnly(2026, 4, 30));

        using var scope = factory.Services.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ExecuteMonthlyAccrualHandler>();

        var januaryResult = await handler.HandleAsync(new ExecuteMonthlyAccrualCommand(new DateOnly(2026, 3, 1)), CancellationToken.None);
        var aprilResult = await handler.HandleAsync(new ExecuteMonthlyAccrualCommand(new DateOnly(2026, 6, 1)), CancellationToken.None);

        Assert.True(januaryResult.IsSuccess);
        Assert.True(aprilResult.IsSuccess);
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        var day31Balance = db.VacationBalances.Single(balance => balance.UserId == "user-day-31");
        var monthEndBalance = db.VacationBalances.Single(balance => balance.UserId == "user-month-end");
        Assert.Equal(4, day31Balance.AccruedDays);
        Assert.Equal(1, monthEndBalance.AccruedDays);
        Assert.DoesNotContain(db.BalanceMovements, movement =>
            movement.BalanceId == day31Balance.Id &&
            movement.Type == MovementType.Accrual &&
            movement.AccrualPeriod == new DateOnly(2026, 1, 1));
        Assert.DoesNotContain(db.BalanceMovements, movement =>
            movement.BalanceId == monthEndBalance.Id &&
            movement.Type == MovementType.Accrual &&
            movement.AccrualPeriod == new DateOnly(2026, 4, 1));
    }
}
