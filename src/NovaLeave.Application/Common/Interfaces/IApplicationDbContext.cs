using NovaLeave.Domain.Entities;

namespace NovaLeave.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    IQueryable<VacationRequest> VacationRequests { get; }

    IQueryable<VacationBalance> VacationBalances { get; }

    IQueryable<BalanceMovement> BalanceMovements { get; }

    IQueryable<AuditRecord> AuditRecords { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
