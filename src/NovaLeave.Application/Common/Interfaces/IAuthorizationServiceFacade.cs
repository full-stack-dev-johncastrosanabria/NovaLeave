namespace NovaLeave.Application.Common.Interfaces;

public interface IAuthorizationServiceFacade
{
    Task<bool> IsActiveUserAsync(string userId, CancellationToken cancellationToken = default);

    Task<bool> CanResolveRequestAsync(string actorId, string ownerId, CancellationToken cancellationToken = default);

    Task<bool> IsActiveHrAsync(string userId, CancellationToken cancellationToken = default);
}
