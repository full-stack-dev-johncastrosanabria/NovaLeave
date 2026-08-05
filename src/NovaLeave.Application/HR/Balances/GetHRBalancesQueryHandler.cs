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
        var totalCount = _dbContext.VacationBalances.Count();
        var pagination = PaginationParameters.Normalize(query.Page, query.PageSize, totalCount);
        var balances = _dbContext.VacationBalances
            .OrderBy(balance => balance.UserId)
            .Skip(pagination.Offset)
            .Take(pagination.PageSize)
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

        return new PagedResult<HRBalanceSummary>(items, pagination.Page, pagination.PageSize, totalCount);
    }
}
