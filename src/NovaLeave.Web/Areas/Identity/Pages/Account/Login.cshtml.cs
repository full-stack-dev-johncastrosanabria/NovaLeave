using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using NovaLeave.Application.Configuration;
using NovaLeave.Infrastructure.Identity;

namespace NovaLeave.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
public sealed class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly NovaLeaveOptions _options;

    public LoginModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IOptions<NovaLeaveOptions> options)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _options = options.Value;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public IReadOnlyList<DemoAccountOption> DemoAccounts { get; private set; } = [];

    public async Task OnGetAsync()
    {
        DemoAccounts = await GetConfiguredDemoAccountsAsync();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        DemoAccounts = await GetConfiguredDemoAccountsAsync();
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            return LocalRedirect(returnUrl ?? await ResolveLandingRouteAsync(Input.Email));
        }

        ModelState.AddModelError(string.Empty, "Credenciales invalidas.");
        return Page();
    }

    public sealed class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public sealed record DemoAccountOption(string Label, string Email);

    private async Task<IReadOnlyList<DemoAccountOption>> GetConfiguredDemoAccountsAsync()
    {
        if (!_options.SeedDemoUsers)
        {
            return [];
        }

        var candidates = new[]
        {
            new DemoAccountOption("Usuario Demo", "demo.user@novaleave.test"),
            new DemoAccountOption("Aprobador Demo", "demo.approver@novaleave.test"),
            new DemoAccountOption("RRHH Demo", "demo.rrhh@novaleave.test"),
            new DemoAccountOption("Multirrol Demo", "demo.combo@novaleave.test")
        };

        var configured = new List<DemoAccountOption>();
        foreach (var candidate in candidates)
        {
            if (await _userManager.FindByEmailAsync(candidate.Email) is not null)
            {
                configured.Add(candidate);
            }
        }

        return configured;
    }

    private async Task<string> ResolveLandingRouteAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return "/mis-solicitudes";
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains("User", StringComparer.Ordinal))
        {
            return "/mis-solicitudes";
        }

        if (roles.Contains("Approver", StringComparer.Ordinal) && user.CanResolveRequests)
        {
            return "/aprobaciones";
        }

        return roles.Contains("HR", StringComparer.Ordinal) ? "/rrhh" : "/Identity/Account/AccessDenied";
    }
}
