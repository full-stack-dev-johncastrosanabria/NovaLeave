using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.WebEncoders;
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
    .Validate(options => !string.IsNullOrWhiteSpace(options.AccrualSchedulerCadence), "Accrual scheduler cadence is required.")
    .Validate(options => NovaLeaveOptions.TryGetDailyUtcTime(options.TimeoutSchedulerCadence, out _), "Timeout scheduler cadence must use format 'Daily HH:mm UTC'.")
    .ValidateOnStart();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton(provider => provider.GetRequiredService<IOptions<NovaLeaveOptions>>().Value);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Spanish accents (á é í ó ú ñ ¿ ¡) are emitted as literal UTF-8 rather than numeric entities.
// The default encoder escapes everything outside Basic Latin, which made the markup unreadable
// and larger. Only the Latin-1 supplement is added; HTML-significant characters are still
// escaped, so Razor's XSS protection is unchanged (constitution §7.2).
builder.Services.Configure<WebEncoderOptions>(options =>
{
    options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement);
});

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
app.UseMiddleware<CorrelationMiddleware>();
app.UseSerilogRequestLogging();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// The MVP defines no landing page, so "/" had no route and returned 404 after
// sign-in (the login ReturnUrl is "/"). Send each identity to the first entry
// point its roles allow. Presentation-level routing only: no business rules.
app.MapGet("/", (HttpContext http) =>
    http.User.IsInRole("User") ? Results.Redirect("/mis-solicitudes")
    : http.User.IsInRole("Approver") ? Results.Redirect("/aprobaciones")
    : http.User.IsInRole("HR") ? Results.Redirect("/rrhh")
    : Results.Redirect("/Identity/Account/AccessDenied"));

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
