using NovaLeave.Domain.ValueObjects;

namespace NovaLeave.Domain.Services;

public sealed class MonthlyAccrualPolicy
{
    public IReadOnlyList<AccrualPeriod> GetEligiblePeriods(DateOnly employmentStartDate, DateOnly referenceDate)
    {
        var firstEligibleMonth = employmentStartDate.Day == 1
            ? new DateOnly(employmentStartDate.Year, employmentStartDate.Month, 1)
            : new DateOnly(employmentStartDate.Year, employmentStartDate.Month, 1).AddMonths(1);

        var lastCompletedMonth = new DateOnly(referenceDate.Year, referenceDate.Month, 1).AddMonths(-1);
        if (lastCompletedMonth < firstEligibleMonth)
        {
            return [];
        }

        var periods = new List<AccrualPeriod>();
        for (var period = firstEligibleMonth; period <= lastCompletedMonth; period = period.AddMonths(1))
        {
            periods.Add(new AccrualPeriod(period.Year, period.Month));
        }

        return periods;
    }

    public static DateOnly ToDateOnly(AccrualPeriod period)
    {
        return new DateOnly(period.Year, period.Month, 1);
    }
}
