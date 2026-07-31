using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLeave.Application;
using NovaLeave.Application.Common.Interfaces;
using NovaLeave.Application.Configuration;
using NovaLeave.Infrastructure;
using NovaLeave.Web.Filters;
using NovaLeave.Web.Services;
using Serilog;

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
    .ValidateOnStart();

builder.Services.AddSingleton(TimeProvider.System);
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
    options.AddPolicy("RequireActiveHR", policy => policy.RequireAuthenticatedUser().RequireRole("HR").RequireClaim("IsActive", "true"));
});

var app = builder.Build();

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
