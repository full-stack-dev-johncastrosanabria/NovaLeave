using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NovaLeave.Application.Configuration;
using NovaLeave.Domain.Entities;
using NovaLeave.Infrastructure.Persistence;

namespace NovaLeave.Infrastructure.Identity;

public sealed class DemoUserSeeder : IHostedService
{
    private static readonly DemoIdentity[] Identities =
    [
        new("demo.user@novaleave.test", ["User", "Approver"], false),
        new("demo.approver@novaleave.test", ["Approver"], true),
        new("demo.rrhh@novaleave.test", ["HR"], false),
        new("demo.combo@novaleave.test", ["User", "Approver", "HR"], true)
    ];

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHostEnvironment _environment;
    private readonly NovaLeaveOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<DemoUserSeeder> _logger;

    public DemoUserSeeder(
        IServiceScopeFactory scopeFactory,
        IHostEnvironment environment,
        IOptions<NovaLeaveOptions> options,
        TimeProvider timeProvider,
        ILogger<DemoUserSeeder> logger)
    {
        _scopeFactory = scopeFactory;
        _environment = environment;
        _options = options.Value;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment() || !_options.SeedDemoUsers)
        {
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        var seedDate = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);

        foreach (var role in new[] { "User", "Approver", "HR" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                EnsureSucceeded(await roleManager.CreateAsync(new IdentityRole(role)), $"create role '{role}'");
            }
        }

        foreach (var identity in Identities)
        {
            var user = await userManager.FindByEmailAsync(identity.Email);
            if (user is null)
            {
                user = new ApplicationUser
                {
                    UserName = identity.Email,
                    Email = identity.Email,
                    EmailConfirmed = true,
                    IsActive = true,
                    CanResolveRequests = identity.CanResolveRequests,
                    EmploymentStartDate = seedDate.AddMonths(-12)
                };
                EnsureSucceeded(await userManager.CreateAsync(user, _options.DemoUserPassword), $"create demo user '{identity.Email}'");
            }
            else
            {
                var changed = false;
                if (!user.IsActive)
                {
                    user.IsActive = true;
                    changed = true;
                }

                if (user.CanResolveRequests != identity.CanResolveRequests)
                {
                    user.CanResolveRequests = identity.CanResolveRequests;
                    changed = true;
                }

                if (user.EmploymentStartDate == default)
                {
                    user.EmploymentStartDate = seedDate.AddMonths(-12);
                    changed = true;
                }

                if (changed)
                {
                    EnsureSucceeded(await userManager.UpdateAsync(user), $"update demo user '{identity.Email}'");
                }
            }

            var currentRoles = await userManager.GetRolesAsync(user);
            var missingRoles = identity.Roles.Except(currentRoles, StringComparer.Ordinal).ToArray();
            if (missingRoles.Length > 0)
            {
                EnsureSucceeded(await userManager.AddToRolesAsync(user, missingRoles), $"assign demo roles to '{identity.Email}'");
            }

            if (identity.Roles.Contains("User", StringComparer.Ordinal) &&
                !await db.VacationBalances.AnyAsync(balance => balance.UserId == user.Id, cancellationToken))
            {
                var timestamp = _timeProvider.GetUtcNow();
                var balance = VacationBalance.Create(user.Id, timestamp);
                for (var month = 1; month <= 12; month++)
                {
                    balance.ApplyAccrual(seedDate.AddMonths(-12 + month), "System", timestamp);
                }

                db.VacationBalances.Add(balance);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Development demo identities are available.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static void EnsureSucceeded(IdentityResult result, string operation)
    {
        if (!result.Succeeded)
        {
            var codes = string.Join(", ", result.Errors.Select(error => error.Code));
            throw new InvalidOperationException($"Unable to {operation}. Identity errors: {codes}.");
        }
    }

    private sealed record DemoIdentity(string Email, string[] Roles, bool CanResolveRequests);
}
