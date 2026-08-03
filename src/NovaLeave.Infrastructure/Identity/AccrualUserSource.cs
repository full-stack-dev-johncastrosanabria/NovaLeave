using Microsoft.EntityFrameworkCore;
using NovaLeave.Application.Common.Interfaces;
using NovaLeave.Infrastructure.Persistence;

namespace NovaLeave.Infrastructure.Identity;

public sealed class AccrualUserSource : IAccrualUserSource
{
    private readonly NovaLeaveDbContext _dbContext;

    public AccrualUserSource(NovaLeaveDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AccrualUser>> GetActiveUsersAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(user =>
                user.IsActive &&
                _dbContext.UserRoles.Any(userRole =>
                    userRole.UserId == user.Id &&
                    _dbContext.Roles.Any(role => role.Id == userRole.RoleId && role.Name == "User")))
            .Select(user => new AccrualUser(user.Id, user.EmploymentStartDate))
            .ToListAsync(cancellationToken);
    }
}
