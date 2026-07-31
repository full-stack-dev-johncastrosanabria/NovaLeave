namespace NovaLeave.Domain.ValueObjects;

public readonly record struct DateRange
{
    public DateRange(DateOnly start, DateOnly end)
    {
        if (start > end)
        {
            throw new ArgumentException("Start date cannot be after end date.", nameof(start));
        }

        Start = start;
        End = end;
    }

    public DateOnly Start { get; }

    public DateOnly End { get; }

    public bool Overlaps(DateRange other)
    {
        return Start <= other.End && other.Start <= End;
    }
}
