using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NovaLeave.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
public sealed class AccessDeniedModel : PageModel
{
    public void OnGet()
    {
    }
}
