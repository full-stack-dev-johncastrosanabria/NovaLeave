using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NovaLeave.Infrastructure;
using NovaLeave.Infrastructure.Scheduling;

namespace NovaLeave.IntegrationTests.Configuration;

public sealed class TimeoutJobConfigurationTests
{
    [Fact]
    public void AddInfrastructure_Registers_Timeout_Job_When_Required_Configuration_Is_Valid()
    {
        var services = new ServiceCollection();

        services.AddInfrastructure(BuildConfiguration(("NovaLeave:PendingRequestTimeoutDays", "14"), ("NovaLeave:TimeoutSchedulerCadence", "Daily 00:05 UTC")));

        Assert.Contains(services, service =>
            service.ServiceType == typeof(IHostedService) &&
            service.ImplementationType == typeof(PendingRequestTimeoutJob));
    }

    [Theory]
    [InlineData("", "Daily 00:05 UTC")]
    [InlineData("14", "")]
    [InlineData("0", "Daily 00:05 UTC")]
    [InlineData("14", "Daily")]
    public void AddInfrastructure_Does_Not_Register_Timeout_Job_When_Required_Configuration_Is_Missing_Or_Invalid(
        string timeoutDays,
        string cadence)
    {
        var services = new ServiceCollection();

        services.AddInfrastructure(BuildConfiguration(("NovaLeave:PendingRequestTimeoutDays", timeoutDays), ("NovaLeave:TimeoutSchedulerCadence", cadence)));

        Assert.DoesNotContain(services, service =>
            service.ServiceType == typeof(IHostedService) &&
            service.ImplementationType == typeof(PendingRequestTimeoutJob));
    }

    private static IConfiguration BuildConfiguration(params (string Key, string Value)[] values)
    {
        var configuration = values.ToDictionary(value => value.Key, value => (string?)value.Value);
        configuration["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=NovaLeave_ConfigTest;Trusted_Connection=True;";
        return new ConfigurationBuilder().AddInMemoryCollection(configuration).Build();
    }
}
