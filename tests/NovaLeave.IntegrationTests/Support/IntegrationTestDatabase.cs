using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NovaLeave.Domain.Entities;
using NovaLeave.Infrastructure.Identity;
using NovaLeave.Infrastructure.Persistence;

namespace NovaLeave.IntegrationTests.Support;

public static class IntegrationTestDatabase
{
    public static async Task ResetAsync(NovaLeaveWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();
    }

    public static async Task SeedUserAsync(
        NovaLeaveWebApplicationFactory factory,
        string userId,
        string email,
        int accruedDays,
        bool isActive = true,
        bool canResolveRequests = false,
        string roles = "User",
        DateOnly? employmentStartDate = null)
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();

        foreach (var role in roles.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole(role));
                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException(string.Join("; ", roleResult.Errors.Select(error => error.Description)));
                }
            }
        }

        var user = new ApplicationUser
        {
            Id = userId,
            UserName = email,
            Email = email,
            NormalizedUserName = email.ToUpperInvariant(),
            NormalizedEmail = email.ToUpperInvariant(),
            EmailConfirmed = true,
            IsActive = isActive,
            CanResolveRequests = canResolveRequests,
            EmploymentStartDate = employmentStartDate ?? new DateOnly(2026, 1, 1)
        };

        var result = await userManager.CreateAsync(user, "Demo123!");
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => error.Description)));
        }

        foreach (var role in roles.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            await userManager.AddToRoleAsync(user, role);
        }

        var balance = VacationBalance.Create(userId, DateTimeOffset.UtcNow);
        for (var i = 0; i < accruedDays; i++)
        {
            balance.ApplyAccrual(new DateOnly(2026, Math.Min(i + 1, 12), 1), "System", DateTimeOffset.UtcNow);
        }

        db.VacationBalances.Add(balance);
        await db.SaveChangesAsync();
    }

    public static HttpRequestMessage AuthenticatedGet(
        string path,
        string userId = "user-1",
        string roles = "User",
        bool isActive = true,
        bool canResolveRequests = false)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        AddUserHeaders(request, userId, roles, isActive, canResolveRequests);
        return request;
    }

    public static HttpRequestMessage AuthenticatedPost(
        string path,
        HttpContent content,
        string userId = "user-1",
        string roles = "User",
        bool isActive = true,
        bool canResolveRequests = false)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, path) { Content = content };
        AddUserHeaders(request, userId, roles, isActive, canResolveRequests);
        return request;
    }

    private static void AddUserHeaders(HttpRequestMessage request, string userId, string roles, bool isActive, bool canResolveRequests)
    {
        request.Headers.Add("X-Test-UserId", userId);
        request.Headers.Add("X-Test-Roles", roles);
        request.Headers.Add("X-Test-IsActive", isActive ? "true" : "false");
        if (roles.Contains("Approver", StringComparison.OrdinalIgnoreCase))
        {
            request.Headers.Add("X-Test-CanResolveRequests", canResolveRequests ? "true" : "false");
        }
    }
}
