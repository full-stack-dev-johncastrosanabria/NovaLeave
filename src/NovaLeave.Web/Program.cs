using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLeave.Application;
using NovaLeave.Application.Common.Interfaces;
using NovaLeave.Application.Configuration;
using NovaLeave.Infrastructure;
using NovaLeave.Web.Filters;
using NovaLeave.Web.Services;
using Serilog;
using Microsoft.Extensions.Options;
using NovaLeave.Application.Authorization;
using NovaLeave.Web.Middleware;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

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
    .Validate(options => NovaLeaveOptions.TryGetDailyUtcTime(options.AccrualSchedulerCadence, out _), "Accrual scheduler cadence must use format 'Daily HH:mm UTC'.")
    .Validate(options => NovaLeaveOptions.TryGetDailyUtcTime(options.TimeoutSchedulerCadence, out _), "Timeout scheduler cadence must use format 'Daily HH:mm UTC'.")
    .Validate(options => !options.SeedDemoUsers || !string.IsNullOrWhiteSpace(options.DemoUserPassword), "Demo user password is required when demo seeding is enabled.")
    .ValidateOnStart();

builder.Services
    .AddOptions<DatabaseConfigurationOptions>()
    .Configure(options => options.DefaultConnection = builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty)
    .Validate(options => !string.IsNullOrWhiteSpace(options.DefaultConnection), "Required configuration 'ConnectionStrings:DefaultConnection' is missing or empty.")
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
    options.Filters.Add<SafeExceptionFilter>();
});
builder.Services.AddRazorPages();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(builder.Configuration.GetValue<int>("NovaLeave:SessionTimeoutMinutes"));
    options.SlidingExpiration = true;
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

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseMiddleware<CorrelationMiddleware>();
app.UseSerilogRequestLogging();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapGet("/health/live", () => Results.Json(new
{
    status = HealthStatus.Healthy.ToString()
})).AllowAnonymous();
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var payload = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString()
            })
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}).AllowAnonymous();
app.MapDefaultControllerRoute();

app.Run();

public partial class Program;

internal sealed class DatabaseConfigurationOptions
{
    public string DefaultConnection { get; set; } = string.Empty;
}
