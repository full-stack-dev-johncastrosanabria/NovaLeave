using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NovaLeave.Application.Authorization;

namespace NovaLeave.Infrastructure.Identity;

public sealed class ApproverIdentityService : IApproverIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ApproverIdentityService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ApproverIdentity?> GetApproverAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await _userManager.Users.SingleOrDefaultAsync(candidate => candidate.Id == userId, cancellationToken);
        if (user is null || !await _userManager.IsInRoleAsync(user, "Approver"))
        {
            return null;
        }

        return new ApproverIdentity(user.Id, user.IsActive, user.CanResolveRequests);
    }
}
