using NovaLeave.Application.Common.Interfaces;
using NovaLeave.Application.Common.Models;

namespace NovaLeave.Application.HR.Balances;

public sealed record GetHRBalancesQuery(int Page = 1, int PageSize = 50);

public sealed record HRBalanceSummary(
    string UserId,
    string UserName,
    int AccruedDays,
    int ReservedDays,
    int DeductedDays,
    int AvailableDays);

public sealed class GetHRBalancesQueryHandler
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserDirectory _userDirectory;

    public GetHRBalancesQueryHandler(IApplicationDbContext dbContext, IUserDirectory userDirectory)
    {
        _dbContext = dbContext;
        _userDirectory = userDirectory;
    }

    public async Task<PagedResult<HRBalanceSummary>> HandleAsync(GetHRBalancesQuery query, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Max(1, query.PageSize);
        var balances = _dbContext.VacationBalances
            .OrderBy(balance => balance.UserId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var users = await _userDirectory.GetUsersByIdsAsync(balances.Select(balance => balance.UserId).ToArray(), cancellationToken);
        var items = balances
            .Select(balance => new HRBalanceSummary(
                balance.UserId,
                users.TryGetValue(balance.UserId, out var user) ? user.DisplayName : balance.UserId,
                balance.AccruedDays,
                balance.ReservedDays,
                balance.DeductedDays,
                balance.AvailableDays))
            .ToList();

        return new PagedResult<HRBalanceSummary>(items, page, pageSize, _dbContext.VacationBalances.Count());
    }
}
