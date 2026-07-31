using NovaLeave.Domain.Enums;

namespace NovaLeave.Domain.Entities;

public sealed class BalanceMovement
{
    private BalanceMovement()
    {
        ActorId = string.Empty;
    }

    private BalanceMovement(
        Guid balanceId,
        Guid? requestId,
        MovementType type,
        int amount,
        string actorId,
        DateOnly? accrualPeriod,
        DateTimeOffset timestampUtc)
    {
        Id = Guid.NewGuid();
        BalanceId = balanceId;
        RequestId = requestId;
        Type = type;
        Amount = amount > 0 ? amount : throw new ArgumentOutOfRangeException(nameof(amount));
        ActorId = actorId;
        AccrualPeriod = accrualPeriod;
        EffectiveAtUtc = timestampUtc.UtcDateTime;
        CreatedAtUtc = timestampUtc.UtcDateTime;
    }

    public Guid Id { get; private set; }

    public Guid BalanceId { get; private set; }

    public Guid? RequestId { get; private set; }

    public MovementType Type { get; private set; }

    public int Amount { get; private set; }

    public string ActorId { get; private set; }

    public DateOnly? AccrualPeriod { get; private set; }

    public DateTime EffectiveAtUtc { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public static BalanceMovement Create(
        Guid balanceId,
        Guid? requestId,
        MovementType type,
        int amount,
        string actorId,
        DateOnly? accrualPeriod,
        DateTimeOffset timestampUtc)
    {
        return new BalanceMovement(balanceId, requestId, type, amount, actorId, accrualPeriod, timestampUtc);
    }
}
