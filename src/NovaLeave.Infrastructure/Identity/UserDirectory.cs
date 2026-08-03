using Microsoft.EntityFrameworkCore;
using NovaLeave.Application.Common.Interfaces;
using NovaLeave.Infrastructure.Persistence;

namespace NovaLeave.Infrastructure.Identity;

public sealed class UserDirectory : IUserDirectory
{
    private readonly NovaLeaveDbContext _dbContext;

    public UserDirectory(NovaLeaveDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyDictionary<string, UserDirectoryEntry>> GetUsersByIdsAsync(IReadOnlyCollection<string> userIds, CancellationToken cancellationToken)
    {
        if (userIds.Count == 0)
        {
            return new Dictionary<string, UserDirectoryEntry>();
        }

        return await _dbContext.Users
            .Where(user => userIds.Contains(user.Id))
            .Select(user => new UserDirectoryEntry(user.Id, user.Email ?? user.UserName ?? user.Id))
            .ToDictionaryAsync(user => user.Id, cancellationToken);
    }

    public async Task<IReadOnlyList<UserDirectoryEntry>> GetAllUsersAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .OrderBy(user => user.Email)
            .Select(user => new UserDirectoryEntry(user.Id, user.Email ?? user.UserName ?? user.Id))
            .ToListAsync(cancellationToken);
    }
}
