using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.Security;

public sealed class AuthorizationPolicyTests
{
    [Theory]
    [InlineData("RequireActiveUser")]
    [InlineData("RequireActiveApprover")]
    [InlineData("RequireActiveHR")]
    public async Task Required_Deny_By_Default_Policies_Are_Registered(string policyName)
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        var provider = factory.Services.GetRequiredService<IAuthorizationPolicyProvider>();

        var policy = await provider.GetPolicyAsync(policyName);

        Assert.NotNull(policy);
        Assert.True(policy!.Requirements.Count > 0);
    }

    [Fact]
    public async Task Approver_Policy_Requires_Approver_Role_And_CanResolveRequests_Claim()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        var provider = factory.Services.GetRequiredService<IAuthorizationPolicyProvider>();

        var policy = await provider.GetPolicyAsync("RequireActiveApprover");

        Assert.NotNull(policy);
        Assert.Contains(policy!.Requirements, requirement => requirement.GetType().Name.Contains("RolesAuthorizationRequirement", StringComparison.Ordinal));
        Assert.Contains(policy.Requirements, requirement => requirement.GetType().Name.Contains("ClaimsAuthorizationRequirement", StringComparison.Ordinal));
    }
}
