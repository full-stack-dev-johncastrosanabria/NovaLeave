using System.ComponentModel.DataAnnotations;
using NovaLeave.Application.Configuration;

namespace NovaLeave.UnitTests.Configuration;

public sealed class NovaLeaveOptionsTests
{
    [Fact]
    public void Valid_Options_Accept_Required_Positive_Values_And_Cadence()
    {
        var options = new NovaLeaveOptions
        {
            PendingRequestTimeoutDays = 14,
            SessionTimeoutMinutes = 30,
            AccrualSchedulerCadence = "Daily 00:05 UTC",
            TimeoutSchedulerCadence = "Daily 00:05 UTC"
        };

        var results = Validate(options);

        Assert.Empty(results);
    }

    [Theory]
    [InlineData(0, 30, "Daily 00:05 UTC", "Daily 00:05 UTC")]
    [InlineData(14, 0, "Daily 00:05 UTC", "Daily 00:05 UTC")]
    [InlineData(14, 30, "", "Daily 00:05 UTC")]
    [InlineData(14, 30, "Daily 00:05 UTC", "")]
    public void Invalid_Options_Fail_Fast(int timeoutDays, int sessionMinutes, string accrualCadence, string timeoutCadence)
    {
        var options = new NovaLeaveOptions
        {
            PendingRequestTimeoutDays = timeoutDays,
            SessionTimeoutMinutes = sessionMinutes,
            AccrualSchedulerCadence = accrualCadence,
            TimeoutSchedulerCadence = timeoutCadence
        };

        var results = Validate(options);

        Assert.NotEmpty(results);
    }

    private static List<ValidationResult> Validate(NovaLeaveOptions options)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(options, new ValidationContext(options), results, validateAllProperties: true);
        return results;
    }
}
