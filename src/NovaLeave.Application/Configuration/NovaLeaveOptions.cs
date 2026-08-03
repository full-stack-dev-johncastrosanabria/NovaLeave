using System.ComponentModel.DataAnnotations;

namespace NovaLeave.Application.Configuration;

public sealed class NovaLeaveOptions
{
    public const string SectionName = "NovaLeave";

    [Range(1, int.MaxValue)]
    public int PendingRequestTimeoutDays { get; init; }

    [Range(1, int.MaxValue)]
    public int SessionTimeoutMinutes { get; init; }

    [Required]
    public string AccrualSchedulerCadence { get; init; } = string.Empty;

    [Required]
    public string TimeoutSchedulerCadence { get; init; } = string.Empty;

    public bool SeedDemoUsers { get; init; }

    public string DemoUserPassword { get; init; } = string.Empty;

    public static bool TryGetDailyUtcTime(string cadence, out TimeOnly time)
    {
        time = default;
        const string prefix = "Daily ";
        const string suffix = " UTC";

        if (!cadence.StartsWith(prefix, StringComparison.Ordinal) ||
            !cadence.EndsWith(suffix, StringComparison.Ordinal))
        {
            return false;
        }

        var value = cadence[prefix.Length..^suffix.Length];
        return TimeOnly.TryParseExact(value, "HH:mm", out time);
    }
}
