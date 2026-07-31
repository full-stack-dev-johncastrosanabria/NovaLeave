using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Authorization;
using NovaLeave.Infrastructure.Persistence;
using NovaLeave.Web.Filters;

namespace NovaLeave.IntegrationTests.Support;

public sealed class NovaLeaveWebApplicationFactory : WebApplicationFactory<Program>
{
    public NovaLeaveWebApplicationFactory()
    {
        ClientOptions.AllowAutoRedirect = false;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

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

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<NovaLeaveDbContext>>();
            services.AddDbContext<NovaLeaveDbContext>(options =>
                options.UseSqlServer(SqlServerFixture.DefaultConnectionString)
                    .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning)));
            services.Configure<MvcOptions>(options =>
            {
                var antiforgeryFilter = options.Filters.FirstOrDefault(filter =>
                    filter is AutoValidateAntiforgeryTokenAttribute);
                if (antiforgeryFilter is not null)
                {
                    options.Filters.Remove(antiforgeryFilter);
                }

                var safeExceptionFilter = options.Filters.FirstOrDefault(filter =>
                    filter.GetType().FullName?.Contains(nameof(SafeExceptionFilter), StringComparison.Ordinal) == true ||
                    filter.ToString()?.Contains(nameof(SafeExceptionFilter), StringComparison.Ordinal) == true);
                if (safeExceptionFilter is not null)
                {
                    options.Filters.Remove(safeExceptionFilter);
                }
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                options.DefaultScheme = TestAuthHandler.SchemeName;
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });
    }
}
