using NovaLeave.Domain.Entities;
using NovaLeave.Domain.Enums;
using NovaLeave.Domain.Exceptions;

namespace NovaLeave.UnitTests.Domain;

public sealed class VacationBalanceTests
{
    [Fact]
    public void New_Balance_Starts_With_Non_Negative_Totals()
    {
        var balance = VacationBalance.Create("user-1", DateTimeOffset.UtcNow);

        Assert.Equal(0, balance.AccruedDays);
        Assert.Equal(0, balance.ReservedDays);
        Assert.Equal(0, balance.DeductedDays);
        Assert.Equal(0, balance.AvailableDays);
    }

    [Fact]
    public void Reservation_Cannot_Make_Available_Balance_Negative()
    {
        var balance = VacationBalance.Create("user-1", DateTimeOffset.UtcNow);

        var exception = Assert.Throws<InsufficientBalanceException>(
            () => balance.Reserve(Guid.NewGuid(), 1, "user-1", DateTimeOffset.UtcNow));

        Assert.Equal(0, exception.AvailableDays);
        Assert.Equal(1, exception.RequestedDays);
        Assert.Equal(0, balance.ReservedDays);
        Assert.Equal(0, balance.AvailableDays);
    }

    [Fact]
    public void Deduction_Converts_Reservation_Without_Negative_Totals()
    {
        var balance = VacationBalance.Create("user-1", DateTimeOffset.UtcNow);
        var requestId = Guid.NewGuid();

        balance.ApplyAccrual(new DateOnly(2027, 1, 1), "System", DateTimeOffset.UtcNow);
        balance.Reserve(requestId, 1, "user-1", DateTimeOffset.UtcNow);
        balance.Deduct(requestId, 1, "approver-1", DateTimeOffset.UtcNow);

        Assert.Equal(1, balance.AccruedDays);
        Assert.Equal(0, balance.ReservedDays);
        Assert.Equal(1, balance.DeductedDays);
        Assert.Equal(0, balance.AvailableDays);
        Assert.Contains(balance.Movements, movement => movement.Type == MovementType.Deduction);
    }

    [Fact]
    public void MovementType_Does_Not_Define_Adjustment()
    {
        Assert.DoesNotContain("Adjustment", Enum.GetNames<MovementType>());
    }
}
