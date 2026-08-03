using NovaLeave.Domain.Services;
using NovaLeave.Domain.ValueObjects;

namespace NovaLeave.UnitTests.Domain;

public sealed class MonthlyAccrualPolicyTests
{
    [Fact]
    public void MidMonth_Hire_Skips_First_Partial_Month_And_Accrues_First_Completed_Month_On_Next_Month()
    {
        var policy = new MonthlyAccrualPolicy();

        var periods = policy.GetEligiblePeriods(new DateOnly(2026, 3, 15), new DateOnly(2026, 5, 1));

        Assert.Equal([new AccrualPeriod(2026, 4)], periods);
    }

    [Fact]
    public void FirstDay_Hire_Accrues_Hire_Month_When_It_Is_Fully_Completed()
    {
        var policy = new MonthlyAccrualPolicy();

        var periods = policy.GetEligiblePeriods(new DateOnly(2026, 3, 1), new DateOnly(2026, 4, 1));

        Assert.Equal([new AccrualPeriod(2026, 3)], periods);
    }

    [Theory]
    [InlineData(2026, 1, 31, 2026, 3, 1, 2026, 2)]
    [InlineData(2026, 4, 30, 2026, 6, 1, 2026, 5)]
    public void MonthEnd_Hire_Dates_Skip_Hire_Month_Without_Proration(
        int hireYear,
        int hireMonth,
        int hireDay,
        int referenceYear,
        int referenceMonth,
        int referenceDay,
        int expectedYear,
        int expectedMonth)
    {
        var policy = new MonthlyAccrualPolicy();

        var periods = policy.GetEligiblePeriods(
            new DateOnly(hireYear, hireMonth, hireDay),
            new DateOnly(referenceYear, referenceMonth, referenceDay));

        Assert.Equal([new AccrualPeriod(expectedYear, expectedMonth)], periods);
    }

    [Fact]
    public void CatchUp_Returns_Each_Eligible_Completed_Period_Once()
    {
        var policy = new MonthlyAccrualPolicy();

        var periods = policy.GetEligiblePeriods(new DateOnly(2026, 1, 1), new DateOnly(2026, 5, 10));

        Assert.Equal(
            [
                new AccrualPeriod(2026, 1),
                new AccrualPeriod(2026, 2),
                new AccrualPeriod(2026, 3),
                new AccrualPeriod(2026, 4)
            ],
            periods);
    }

    [Fact]
    public void Reference_Date_Inside_Hire_Month_Has_No_Accrual()
    {
        var policy = new MonthlyAccrualPolicy();

        var periods = policy.GetEligiblePeriods(new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31));

        Assert.Empty(periods);
    }
}
