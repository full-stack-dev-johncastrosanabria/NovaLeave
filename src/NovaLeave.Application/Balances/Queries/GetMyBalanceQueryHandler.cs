using NovaLeave.Application.Common.Errors;
using NovaLeave.Application.Common.Interfaces;
using NovaLeave.Application.Common.Results;
using NovaLeave.Domain.Enums;

namespace NovaLeave.Application.Balances.Queries;

public sealed record BalanceMovementItem(DateTime EffectiveAt, MovementType Type, int Amount, Guid? RequestId);

public sealed record MyBalanceView(int AccruedDays, int ReservedDays, int DeductedDays, int AvailableDays, IReadOnlyList<BalanceMovementItem> Movements);

public sealed class GetMyBalanceQueryHandler
{
    private readonly IApplicationDbContext _dbContext;

    public GetMyBalanceQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<MyBalanceView>> HandleAsync(string userId, CancellationToken cancellationToken)
    {
        var balance = _dbContext.VacationBalances
            .SingleOrDefault(candidate => candidate.UserId == userId);

        if (balance is null)
        {
            return Result<MyBalanceView>.Failure(new Error(ErrorCodes.NotFound, "Saldo no encontrado."));
        }

        var movements = _dbContext.BalanceMovements
            .Where(movement => movement.BalanceId == balance.Id)
            .OrderByDescending(movement => movement.EffectiveAtUtc)
            .Select(movement => new BalanceMovementItem(movement.EffectiveAtUtc, movement.Type, movement.Amount, movement.RequestId))
            .ToList();

        return await Task.FromResult(Result<MyBalanceView>.Success(new MyBalanceView(
            balance.AccruedDays,
            balance.ReservedDays,
            balance.DeductedDays,
            balance.AvailableDays,
            movements)));
    }
}
