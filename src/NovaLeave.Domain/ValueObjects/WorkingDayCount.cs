namespace NovaLeave.Domain.ValueObjects;

public readonly record struct WorkingDayCount
{
    private WorkingDayCount(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public static WorkingDayCount From(int value)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "Working day count must be positive.");
        }

        return new WorkingDayCount(value);
    }
}
