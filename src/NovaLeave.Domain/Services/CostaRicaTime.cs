namespace NovaLeave.Domain.Services;

/// <summary>
/// Converts UTC instants to NovaLeave's single business time zone.
/// Persisted timestamps remain UTC; only business-date evaluation and presentation use Costa Rica time.
/// </summary>
public static class CostaRicaTime
{
    public const string TimeZoneId = "America/Costa_Rica";

    private static readonly TimeZoneInfo BusinessTimeZone = ResolveTimeZone();

    public static DateOnly GetBusinessDate(DateTimeOffset instant)
    {
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(instant, BusinessTimeZone).DateTime);
    }

    public static DateTime ToLocalDateTime(DateTime timestampUtc)
    {
        var utc = timestampUtc.Kind switch
        {
            DateTimeKind.Utc => timestampUtc,
            DateTimeKind.Local => timestampUtc.ToUniversalTime(),
            _ => DateTime.SpecifyKind(timestampUtc, DateTimeKind.Utc)
        };

        return TimeZoneInfo.ConvertTimeFromUtc(utc, BusinessTimeZone);
    }

    public static DateTimeOffset ToLocalDateTime(DateTimeOffset timestamp)
    {
        return TimeZoneInfo.ConvertTime(timestamp, BusinessTimeZone);
    }

    private static TimeZoneInfo ResolveTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
        }
    }
}
