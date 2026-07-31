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
        bool canResolveRequests = false)
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();

        if (!await roleManager.RoleExistsAsync("User"))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole("User"));
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", roleResult.Errors.Select(error => error.Description)));
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
            EmploymentStartDate = new DateOnly(2026, 1, 1)
        };

        var result = await userManager.CreateAsync(user, "Demo123!");
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => error.Description)));
        }

        await userManager.AddToRoleAsync(user, "User");

        var balance = VacationBalance.Create(userId, DateTimeOffset.UtcNow);
        for (var i = 0; i < accruedDays; i++)
        {
            balance.ApplyAccrual(new DateOnly(2026, Math.Min(i + 1, 12), 1), "System", DateTimeOffset.UtcNow);
        }

        db.VacationBalances.Add(balance);
        await db.SaveChangesAsync();
    }

    public static HttpRequestMessage AuthenticatedGet(string path, string userId = "user-1")
    {
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        AddUserHeaders(request, userId);
        return request;
    }

    public static HttpRequestMessage AuthenticatedPost(string path, HttpContent content, string userId = "user-1")
    {
        var request = new HttpRequestMessage(HttpMethod.Post, path) { Content = content };
        AddUserHeaders(request, userId);
        return request;
    }

    private static void AddUserHeaders(HttpRequestMessage request, string userId)
    {
        request.Headers.Add("X-Test-UserId", userId);
        request.Headers.Add("X-Test-Roles", "User");
        request.Headers.Add("X-Test-IsActive", "true");
    }
}
