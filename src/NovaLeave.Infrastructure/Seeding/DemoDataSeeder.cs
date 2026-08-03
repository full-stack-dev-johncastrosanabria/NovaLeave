using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NovaLeave.Application.Configuration;
using NovaLeave.Domain.Entities;
using NovaLeave.Domain.Services;
using NovaLeave.Infrastructure.Identity;
using NovaLeave.Infrastructure.Persistence;

namespace NovaLeave.Infrastructure.Seeding;

/// <summary>
/// Seeds the demo identities documented in <c>specs/001-leave-management-mvp/quickstart.md</c>
/// so a freshly created database can be signed into during Development and Staging.
/// </summary>
/// <remarks>
/// Runs only when <see cref="NovaLeaveOptions.SeedDemoUsers"/> is <c>true</c> and the host is not
/// Production. It is idempotent: an identity that already exists is left untouched, so restarting
/// the application never duplicates users, roles, or balance movements.
/// </remarks>
public static class DemoDataSeeder
{
    /// <summary>Password shared by every demo identity. Development and Staging only.</summary>
    private const string DemoPassword = "Demo123!";

    private const string SystemActorId = "System";

    /// <summary>Months of service granted to each demo identity, so balances start non-empty.</summary>
    private const int DemoMonthsOfService = 12;

    /// <remarks>
    /// Approvers and HR staff are employees as well, so they also hold <c>User</c> and can file
    /// their own requests and see their own balance. Constitution §4.6 keeps this safe: requesting
    /// and approving are mutually exclusive <em>per resource</em>, so they still cannot resolve
    /// their own request. Only <c>user@demo</c> is deliberately single-role, to exercise the
    /// navigation and context-switcher rules for an identity with one context (RBFV 4.2).
    /// </remarks>
    private static readonly DemoIdentity[] DemoIdentities =
    [
        new("demo-user", "user@demo", ["User"], CanResolveRequests: false),
        new("demo-approver", "approver@demo", ["User", "Approver"], CanResolveRequests: true),
        new("demo-hr", "hr@demo", ["User", "HR"], CanResolveRequests: false),
        new("demo-multi", "multi@demo", ["User", "Approver", "HR"], CanResolveRequests: true)
    ];

    /// <summary>
    /// Creates the demo roles, identities, and opening balances when seeding is enabled.
    /// </summary>
    /// <param name="services">Root service provider; a scope is created internally.</param>
    /// <param name="isProduction">
    /// When <c>true</c> the seeder returns immediately. Production must never contain demo
    /// identities, independently of configuration.
    /// </param>
    public static async Task SeedAsync(
        IServiceProvider services,
        bool isProduction,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DemoDataSeeder));
        var options = provider.GetRequiredService<NovaLeaveOptions>();

        if (!options.SeedDemoUsers)
        {
            return;
        }

        if (isProduction)
        {
            logger.LogWarning(
                "SeedDemoUsers is enabled but the host is Production; demo identities were NOT seeded.");
            return;
        }

        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
        var db = provider.GetRequiredService<NovaLeaveDbContext>();
        var timeProvider = provider.GetRequiredService<TimeProvider>();

        var nowUtc = timeProvider.GetUtcNow();
        var today = DateOnly.FromDateTime(nowUtc.UtcDateTime);
        // Day 1 so MonthlyAccrualPolicy treats the hire month itself as eligible.
        var employmentStartDate = new DateOnly(today.Year, today.Month, 1).AddMonths(-DemoMonthsOfService);

        foreach (var roleName in DemoIdentities.SelectMany(identity => identity.Roles).Distinct())
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await ThrowIfFailedAsync(
                    roleManager.CreateAsync(new IdentityRole(roleName)),
                    $"create role '{roleName}'");
            }
        }

        var seeded = 0;
        foreach (var identity in DemoIdentities)
        {
            var existing = await userManager.FindByNameAsync(identity.Email);
            if (existing is not null)
            {
                // Reconcile roles so an existing demo database picks up catalogue changes
                // without being recreated. Still idempotent: unchanged identities are untouched.
                var currentRoles = await userManager.GetRolesAsync(existing);
                var missingRoles = identity.Roles.Except(currentRoles, StringComparer.Ordinal).ToArray();
                if (missingRoles.Length > 0)
                {
                    await ThrowIfFailedAsync(
                        userManager.AddToRolesAsync(existing, missingRoles),
                        $"add roles '{string.Join(", ", missingRoles)}' to '{identity.Email}'");
                    logger.LogInformation(
                        "Added roles {Roles} to existing demo identity {Email}.",
                        string.Join(", ", missingRoles),
                        identity.Email);
                }

                continue;
            }

            var user = new ApplicationUser
            {
                Id = identity.Id,
                UserName = identity.Email,
                Email = identity.Email,
                EmailConfirmed = true,
                IsActive = true,
                CanResolveRequests = identity.CanResolveRequests,
                EmploymentStartDate = employmentStartDate
            };

            await ThrowIfFailedAsync(
                userManager.CreateAsync(user, DemoPassword),
                $"create demo user '{identity.Email}'");

            foreach (var roleName in identity.Roles)
            {
                await ThrowIfFailedAsync(
                    userManager.AddToRoleAsync(user, roleName),
                    $"assign role '{roleName}' to '{identity.Email}'");
            }

            await SeedOpeningBalanceAsync(db, identity.Id, employmentStartDate, today, nowUtc, cancellationToken);
            seeded++;
        }

        if (seeded > 0)
        {
            logger.LogInformation(
                "Seeded {Count} demo identities with employment start {EmploymentStartDate}.",
                seeded,
                employmentStartDate);
        }
    }

    /// <summary>
    /// Creates the balance and applies one accrual per completed month of service.
    /// </summary>
    /// <remarks>
    /// Periods come from <see cref="MonthlyAccrualPolicy"/> — the same policy the monthly accrual
    /// job uses — so the job's per-period duplicate check recognises these movements and will not
    /// accrue them a second time.
    /// </remarks>
    private static async Task SeedOpeningBalanceAsync(
        NovaLeaveDbContext db,
        string userId,
        DateOnly employmentStartDate,
        DateOnly today,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken)
    {
        if (await db.VacationBalances.AnyAsync(balance => balance.UserId == userId, cancellationToken))
        {
            return;
        }

        var balance = VacationBalance.Create(userId, nowUtc);

        foreach (var period in new MonthlyAccrualPolicy().GetEligiblePeriods(employmentStartDate, today))
        {
            balance.ApplyAccrual(MonthlyAccrualPolicy.ToDateOnly(period), SystemActorId, nowUtc);
        }

        db.VacationBalances.Add(balance);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task ThrowIfFailedAsync(Task<IdentityResult> operation, string description)
    {
        var result = await operation;
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Demo seeding failed to {description}: {string.Join("; ", result.Errors.Select(error => error.Description))}");
        }
    }

    private sealed record DemoIdentity(string Id, string Email, string[] Roles, bool CanResolveRequests);
}
