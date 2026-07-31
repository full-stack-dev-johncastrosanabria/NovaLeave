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

    public bool SeedDemoUsers { get; init; }
}
