using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NovaLeave.Application.Authorization;
using NovaLeave.Application.Common.Interfaces;
using NovaLeave.Application.Configuration;
using NovaLeave.Application.HR.ApproverCapabilities;
using NovaLeave.Application.Observability;
using NovaLeave.Infrastructure.Identity;
using NovaLeave.Infrastructure.HealthChecks;
using NovaLeave.Infrastructure.Observability;
using NovaLeave.Infrastructure.Persistence;
using NovaLeave.Infrastructure.Scheduling;

namespace NovaLeave.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<NovaLeaveDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        services
            .AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddEntityFrameworkStores<NovaLeaveDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrincipalFactory>();
        services.AddScoped<IApproverIdentityService, ApproverIdentityService>();
        services.AddScoped<IApproverCapabilityStore, ApproverCapabilityStore>();
        services.AddScoped<IAccrualUserSource, AccrualUserSource>();
        services.AddScoped<IUserDirectory, UserDirectory>();
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<NovaLeaveDbContext>());
        services.AddSingleton<IOperationalTelemetry, OperationalTelemetry>();
        services.AddHealthChecks()
            .AddCheck<DatabaseConnectivityHealthCheck>("database", tags: ["ready"]);

        var timeoutCadence = configuration.GetSection(NovaLeaveOptions.SectionName)[nameof(NovaLeaveOptions.TimeoutSchedulerCadence)];
        var timeoutDays = configuration.GetSection(NovaLeaveOptions.SectionName)[nameof(NovaLeaveOptions.PendingRequestTimeoutDays)];
        if (int.TryParse(timeoutDays, out var parsedTimeoutDays) &&
            parsedTimeoutDays > 0 &&
            timeoutCadence is not null &&
            NovaLeaveOptions.TryGetDailyUtcTime(timeoutCadence, out _))
        {
            services.AddHostedService<PendingRequestTimeoutJob>();
        }

        var accrualCadence = configuration.GetSection(NovaLeaveOptions.SectionName)[nameof(NovaLeaveOptions.AccrualSchedulerCadence)];
        if (accrualCadence is not null &&
            NovaLeaveOptions.TryGetDailyUtcTime(accrualCadence, out _))
        {
            services.AddHostedService<MonthlyAccrualJob>();
        }

        return services;
    }
}
