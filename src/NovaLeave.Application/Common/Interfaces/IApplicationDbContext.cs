using NovaLeave.Domain.Entities;

namespace NovaLeave.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    IQueryable<VacationRequest> VacationRequests { get; }

    IQueryable<VacationBalance> VacationBalances { get; }

    IQueryable<BalanceMovement> BalanceMovements { get; }

    IQueryable<AuditRecord> AuditRecords { get; }

    void AddVacationRequest(VacationRequest request);

    void AddBalanceMovement(BalanceMovement movement);

    void AddAuditRecord(AuditRecord auditRecord);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
