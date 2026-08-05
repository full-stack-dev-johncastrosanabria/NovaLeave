using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NovaLeave.Application.Configuration;
using NovaLeave.Infrastructure.Identity;

namespace NovaLeave.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
public sealed class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly NovaLeaveOptions _options;
    private readonly IWebHostEnvironment _environment;

    public LoginModel(
        SignInManager<ApplicationUser> signInManager,
        NovaLeaveOptions options,
        IWebHostEnvironment environment)
    {
        _signInManager = signInManager;
        _options = options;
        _environment = environment;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    /// Whether the demo user list may be rendered. Requires demo seeding to be enabled
    /// <em>and</em> a non-Production host, so demo identifiers are never printed in Production
    /// even if the flag were set there by mistake.
    /// </summary>
    public bool ShowDemoIdentities => _options.SeedDemoUsers && !_environment.IsProduction();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            return LocalRedirect(returnUrl ?? "/mis-solicitudes");
        }

        ModelState.AddModelError(
            string.Empty,
            "El correo o la contraseña no son correctos. Verifica tus datos e inténtalo de nuevo.");
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
}
