namespace NovaLeave.Application.Common.Interfaces;

public sealed record UserDirectoryEntry(string Id, string DisplayName);

public interface IUserDirectory
{
    Task<IReadOnlyDictionary<string, UserDirectoryEntry>> GetUsersByIdsAsync(IReadOnlyCollection<string> userIds, CancellationToken cancellationToken);

    Task<IReadOnlyList<UserDirectoryEntry>> GetAllUsersAsync(CancellationToken cancellationToken);
}
