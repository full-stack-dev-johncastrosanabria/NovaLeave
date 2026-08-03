using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NovaLeave.Infrastructure.Persistence;

namespace NovaLeave.IntegrationTests.Support;

public sealed class AntiforgeryWebApplicationFactory : WebApplicationFactory<Program>
{
    public AntiforgeryWebApplicationFactory()
    {
        ClientOptions.AllowAutoRedirect = false;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = SqlServerFixture.DefaultConnectionString,
                ["NovaLeave:PendingRequestTimeoutDays"] = "14",
                ["NovaLeave:SessionTimeoutMinutes"] = "30",
                ["NovaLeave:AccrualSchedulerCadence"] = "Daily 00:05 UTC",
                ["NovaLeave:TimeoutSchedulerCadence"] = "Daily 00:05 UTC"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<NovaLeaveDbContext>>();
            services.AddDbContext<NovaLeaveDbContext>(options =>
                options.UseSqlServer(SqlServerFixture.DefaultConnectionString)
                    .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning)));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                options.DefaultScheme = TestAuthHandler.SchemeName;
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });
    }
}
