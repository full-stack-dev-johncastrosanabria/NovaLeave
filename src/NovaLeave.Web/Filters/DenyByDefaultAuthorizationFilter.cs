using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NovaLeave.Web.Filters;

public sealed class DenyByDefaultAuthorizationFilter : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var endpoint = context.HttpContext.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() is not null ||
            context.Filters.OfType<IAllowAnonymousFilter>().Any())
        {
            return;
        }

        if (context.ActionDescriptor.EndpointMetadata.OfType<IAuthorizeData>().Any())
        {
            return;
        }

        context.Result = context.HttpContext.User.Identity?.IsAuthenticated == true
            ? new ForbidResult()
            : new ChallengeResult();
        await Task.CompletedTask;
    }
}
