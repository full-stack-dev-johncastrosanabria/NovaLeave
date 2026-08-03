using NovaLeave.Application.Common.Errors;
using NovaLeave.Application.Common.Interfaces;
using NovaLeave.Application.Common.Results;
using NovaLeave.Domain.Enums;

namespace NovaLeave.Application.HR.Balances;

public sealed record HRBalanceMovementItem(DateTime EffectiveAtUtc, MovementType Type, int Amount, Guid? RequestId, DateOnly? AccrualPeriod);

public sealed record HRBalanceMovementView(
    string UserId,
    string UserName,
    int AccruedDays,
    int ReservedDays,
    int DeductedDays,
    int AvailableDays,
    IReadOnlyList<HRBalanceMovementItem> Movements);

public sealed class GetHRBalanceMovementsQueryHandler
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserDirectory _userDirectory;

    public GetHRBalanceMovementsQueryHandler(IApplicationDbContext dbContext, IUserDirectory userDirectory)
    {
        _dbContext = dbContext;
        _userDirectory = userDirectory;
    }

    public async Task<Result<HRBalanceMovementView>> HandleAsync(string userId, CancellationToken cancellationToken)
    {
        var balance = _dbContext.VacationBalances.SingleOrDefault(candidate => candidate.UserId == userId);
        if (balance is null)
        {
            return Result<HRBalanceMovementView>.Failure(new Error(ErrorCodes.NotFound, "Saldo no encontrado."));
        }

        var users = await _userDirectory.GetUsersByIdsAsync([userId], cancellationToken);
        var movements = _dbContext.BalanceMovements
            .Where(movement => movement.BalanceId == balance.Id)
            .OrderByDescending(movement => movement.EffectiveAtUtc)
            .Select(movement => new HRBalanceMovementItem(movement.EffectiveAtUtc, movement.Type, movement.Amount, movement.RequestId, movement.AccrualPeriod))
            .ToList();

        return Result<HRBalanceMovementView>.Success(new HRBalanceMovementView(
            userId,
            users.TryGetValue(userId, out var user) ? user.DisplayName : userId,
            balance.AccruedDays,
            balance.ReservedDays,
            balance.DeductedDays,
            balance.AvailableDays,
            movements));
    }
}
