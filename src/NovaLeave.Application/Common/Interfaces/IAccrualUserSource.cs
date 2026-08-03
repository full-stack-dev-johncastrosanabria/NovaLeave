namespace NovaLeave.Application.Common.Interfaces;

public sealed record AccrualUser(string UserId, DateOnly EmploymentStartDate);

public interface IAccrualUserSource
{
    Task<IReadOnlyList<AccrualUser>> GetActiveUsersAsync(CancellationToken cancellationToken);
}
