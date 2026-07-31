using NovaLeave.Domain.Services;
using NovaLeave.Domain.ValueObjects;

namespace NovaLeave.UnitTests.Domain;

public sealed class WorkingDaysCalculatorTests
{
    [Fact]
    public void Count_Uses_Inclusive_Monday_To_Friday_Dates()
    {
        var range = new DateRange(new DateOnly(2027, 1, 4), new DateOnly(2027, 1, 8));

        var result = WorkingDaysCalculator.Count(range);

        Assert.Equal(5, result.Value);
    }

    [Fact]
    public void Count_Excludes_Saturdays_And_Sundays()
    {
        var range = new DateRange(new DateOnly(2027, 1, 1), new DateOnly(2027, 1, 10));

        var result = WorkingDaysCalculator.Count(range);

        Assert.Equal(6, result.Value);
    }

    [Fact]
    public void Count_Treats_Holidays_As_Working_Days_When_They_Fall_Monday_To_Friday()
    {
        var holiday = new DateOnly(2027, 1, 1);
        var range = new DateRange(holiday, holiday);

        var result = WorkingDaysCalculator.Count(range, new HashSet<DateOnly> { holiday });

        Assert.Equal(1, result.Value);
    }

    [Fact]
    public void Count_Rejects_Zero_Working_Day_Range()
    {
        var range = new DateRange(new DateOnly(2027, 1, 2), new DateOnly(2027, 1, 3));

        Assert.Throws<InvalidOperationException>(() => WorkingDaysCalculator.Count(range));
    }
}
