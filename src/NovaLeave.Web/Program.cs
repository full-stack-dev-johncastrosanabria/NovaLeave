using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLeave.Application;
using NovaLeave.Application.Common.Interfaces;
using NovaLeave.Application.Configuration;
using NovaLeave.Infrastructure;
using NovaLeave.Infrastructure.Seeding;
using NovaLeave.Web.Filters;
using NovaLeave.Web.Services;
using Serilog;
using Microsoft.Extensions.Options;
using NovaLeave.Application.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

builder.Services
    .AddOptions<NovaLeaveOptions>()
    .Bind(builder.Configuration.GetSection(NovaLeaveOptions.SectionName))
    .ValidateDataAnnotations()
    .Validate(options => !string.IsNullOrWhiteSpace(options.AccrualSchedulerCadence), "Accrual scheduler cadence is required.")
    .Validate(options => NovaLeaveOptions.TryGetDailyUtcTime(options.TimeoutSchedulerCadence, out _), "Timeout scheduler cadence must use format 'Daily HH:mm UTC'.")
    .ValidateOnStart();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton(provider => provider.GetRequiredService<IOptions<NovaLeaveOptions>>().Value);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    options.Filters.Add<DenyByDefaultAuthorizationFilter>();
    options.Filters.Add<OverpostingPreventionFilter>();
    options.Filters.Add<ModelStateValidationFilter>();
    options.Filters.Add<SafeExceptionFilter>();
});
builder.Services.AddRazorPages();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.AddPolicy("RequireActiveUser", policy => policy.RequireAuthenticatedUser().RequireRole("User").RequireClaim("IsActive", "true"));
    options.AddPolicy("RequireActiveApprover", policy => policy.RequireAuthenticatedUser().RequireRole("Approver").RequireClaim("IsActive", "true").RequireClaim("CanResolveRequests", "true"));
    options.AddPolicy(HRPolicies.RequireActiveHR, policy => policy.RequireAuthenticatedUser().RequireRole("HR").RequireClaim("IsActive", "true"));
});

var app = builder.Build();

// Composition root only: seeds the documented demo identities when
// NovaLeave:SeedDemoUsers is enabled. No-ops in Production and when the flag is
// off, and is idempotent across restarts.
await DemoDataSeeder.SeedAsync(app.Services, app.Environment.IsProduction());

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSerilogRequestLogging();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapDefaultControllerRoute();

app.Run();

public partial class Program;
