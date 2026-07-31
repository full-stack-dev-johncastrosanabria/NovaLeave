namespace NovaLeave.Domain.ValueObjects;

public readonly record struct AccrualPeriod
{
    public AccrualPeriod(int year, int month)
    {
        if (year < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(year));
        }

        if (month is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(month));
        }

        Year = year;
        Month = month;
    }

    public int Year { get; }

    public int Month { get; }

    public override string ToString()
    {
        return $"{Year:D4}-{Month:D2}";
    }
}
