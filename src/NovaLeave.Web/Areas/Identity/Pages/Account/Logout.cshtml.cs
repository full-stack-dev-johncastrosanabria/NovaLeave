using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NovaLeave.Infrastructure.Identity;

namespace NovaLeave.Web.Areas.Identity.Pages.Account;

[Authorize]
public sealed class LogoutModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public LogoutModel(SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _signInManager.SignOutAsync();
        return LocalRedirect("/Identity/Account/Login");
    }
}
