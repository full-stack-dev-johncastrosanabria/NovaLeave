using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace NovaLeave.IntegrationTests.Support;

public sealed class NovaLeaveWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            var values = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = SqlServerFixture.DefaultConnectionString,
                ["NovaLeave:PendingRequestTimeoutDays"] = "14",
                ["NovaLeave:SessionTimeoutMinutes"] = "30",
                ["NovaLeave:AccrualSchedulerCadence"] = "Daily 00:05 UTC"
            };

            configuration.AddInMemoryCollection(values);
        });
    }
}
