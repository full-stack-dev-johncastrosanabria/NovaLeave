using Microsoft.AspNetCore.Identity;

namespace NovaLeave.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser
{
    public bool IsActive { get; set; }

    public bool CanResolveRequests { get; set; }

    public DateOnly EmploymentStartDate { get; set; }

    public byte[] RowVersion { get; set; } = [];
}
