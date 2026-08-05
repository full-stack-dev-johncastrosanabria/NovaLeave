using NovaLeave.Domain.ValueObjects;

namespace NovaLeave.Domain.Services;

public static class WorkingDaysCalculator
{
    public static WorkingDayCount Count(DateRange range, IReadOnlySet<DateOnly>? holidays = null)
    {
        var count = 0;

        for (var date = range.Start; date <= range.End; date = date.AddDays(1))
        {
            if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                continue;
            }

            // Approved rule: holidays are counted as ordinary working days.
            _ = holidays;
            count++;
        }

        if (count == 0)
        {
            throw new InvalidOperationException("Date range contains zero working days.");
        }

        return WorkingDayCount.From(count);
    }

    public static DateRange FromStartAndWorkingDays(DateOnly startDate, WorkingDayCount workingDays)
    {
        var remaining = workingDays.Value;
        var current = startDate;

        while (true)
        {
            if (current.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
            {
                remaining--;
            }

            if (remaining == 0)
            {
                return new DateRange(startDate, current);
            }

            current = current.AddDays(1);
        }
    }

    public static void EnsureStartsAfterBusinessDate(DateRange range, TimeProvider timeProvider)
    {
        var today = CostaRicaTime.GetBusinessDate(timeProvider.GetUtcNow());
        if (range.Start <= today)
        {
            throw new InvalidOperationException("Start date must be after the current business date.");
        }
    }
}
