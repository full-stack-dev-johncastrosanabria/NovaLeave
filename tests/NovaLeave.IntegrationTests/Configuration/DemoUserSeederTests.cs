using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NovaLeave.Application.Configuration;
using NovaLeave.Infrastructure.Identity;
using NovaLeave.Infrastructure.Persistence;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.Configuration;

public sealed class DemoUserSeederTests
{
    [Fact]
    public async Task Development_Seed_Is_Idempotent_And_Creates_Approved_Identities()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(factory);
        var seeder = new DemoUserSeeder(
            factory.Services.GetRequiredService<IServiceScopeFactory>(),
            new DevelopmentEnvironment(),
            Options.Create(new NovaLeaveOptions
            {
                SeedDemoUsers = true,
                DemoUserPassword = "Demo123!"
            }),
            TimeProvider.System,
            NullLogger<DemoUserSeeder>.Instance);

        await seeder.StartAsync(default);
        await seeder.StartAsync(default);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        Assert.Equal(4, db.Users.Count(user => user.Email != null && user.Email.EndsWith("@novaleave.test")));
        Assert.Equal(2, db.VacationBalances.Count());
        Assert.Equal(24, db.BalanceMovements.Count());

        var approver = await userManager.FindByEmailAsync("demo.approver@novaleave.test");
        Assert.NotNull(approver);
        Assert.True(approver.IsActive);
        Assert.True(approver.CanResolveRequests);
        Assert.Equal(["Approver"], await userManager.GetRolesAsync(approver));
    }

    private sealed class DevelopmentEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "NovaLeave.IntegrationTests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
