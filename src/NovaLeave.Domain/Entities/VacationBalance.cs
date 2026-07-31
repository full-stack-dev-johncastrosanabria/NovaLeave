using NovaLeave.Domain.Enums;

namespace NovaLeave.Domain.Entities;

public sealed class VacationBalance
{
    private readonly List<BalanceMovement> _movements = [];

    private VacationBalance()
    {
        UserId = string.Empty;
        RowVersion = [];
    }

    private VacationBalance(string userId, DateTimeOffset modifiedAtUtc)
    {
        Id = Guid.NewGuid();
        UserId = RequireText(userId, nameof(userId));
        ModifiedAtUtc = modifiedAtUtc.UtcDateTime;
        RowVersion = [];
    }

    public Guid Id { get; private set; }

    public string UserId { get; private set; }

    public int AccruedDays { get; private set; }

    public int ReservedDays { get; private set; }

    public int DeductedDays { get; private set; }

    public DateTime ModifiedAtUtc { get; private set; }

    public byte[] RowVersion { get; private set; }

    public int AvailableDays => AccruedDays - ReservedDays - DeductedDays;

    public IReadOnlyCollection<BalanceMovement> Movements => _movements.AsReadOnly();

    public static VacationBalance Create(string userId, DateTimeOffset modifiedAtUtc)
    {
        return new VacationBalance(userId, modifiedAtUtc);
    }

    public BalanceMovement ApplyAccrual(DateOnly accrualPeriod, string actorId, DateTimeOffset timestampUtc)
    {
        AccruedDays++;
        return AddMovement(null, MovementType.Accrual, 1, actorId, accrualPeriod, timestampUtc);
    }

    public BalanceMovement Reserve(Guid requestId, int days, string actorId, DateTimeOffset timestampUtc)
    {
        EnsurePositive(days);
        ReservedDays += days;
        EnsureNonNegative();
        return AddMovement(requestId, MovementType.Reservation, days, actorId, null, timestampUtc);
    }

    public BalanceMovement Release(Guid requestId, int days, string actorId, DateTimeOffset timestampUtc)
    {
        EnsurePositive(days);
        ReservedDays -= days;
        EnsureNonNegative();
        return AddMovement(requestId, MovementType.Release, days, actorId, null, timestampUtc);
    }

    public BalanceMovement Deduct(Guid requestId, int days, string actorId, DateTimeOffset timestampUtc)
    {
        EnsurePositive(days);
        ReservedDays -= days;
        DeductedDays += days;
        EnsureNonNegative();
        return AddMovement(requestId, MovementType.Deduction, days, actorId, null, timestampUtc);
    }

    public BalanceMovement Restore(Guid requestId, int days, string actorId, DateTimeOffset timestampUtc)
    {
        EnsurePositive(days);
        DeductedDays -= days;
        EnsureNonNegative();
        return AddMovement(requestId, MovementType.Restoration, days, actorId, null, timestampUtc);
    }

    private BalanceMovement AddMovement(
        Guid? requestId,
        MovementType type,
        int days,
        string actorId,
        DateOnly? accrualPeriod,
        DateTimeOffset timestampUtc)
    {
        var movement = BalanceMovement.Create(Id, requestId, type, days, RequireText(actorId, nameof(actorId)), accrualPeriod, timestampUtc);
        _movements.Add(movement);
        ModifiedAtUtc = timestampUtc.UtcDateTime;
        return movement;
    }

    private void EnsureNonNegative()
    {
        if (AccruedDays < 0 || ReservedDays < 0 || DeductedDays < 0 || AvailableDays < 0)
        {
            throw new InvalidOperationException("Vacation balance totals cannot be negative.");
        }
    }

    private static void EnsurePositive(int days)
    {
        if (days <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(days), days, "Days must be positive.");
        }
    }

    private static string RequireText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", parameterName);
        }

        return value.Trim();
    }
}
