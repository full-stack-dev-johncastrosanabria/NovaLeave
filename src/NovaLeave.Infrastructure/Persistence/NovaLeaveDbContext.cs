using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NovaLeave.Application.Common.Interfaces;
using NovaLeave.Domain.Entities;
using NovaLeave.Infrastructure.Identity;

namespace NovaLeave.Infrastructure.Persistence;

public sealed class NovaLeaveDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public NovaLeaveDbContext(DbContextOptions<NovaLeaveDbContext> options)
        : base(options)
    {
    }

    public DbSet<VacationRequest> VacationRequests => Set<VacationRequest>();

    public DbSet<VacationBalance> VacationBalances => Set<VacationBalance>();

    public DbSet<BalanceMovement> BalanceMovements => Set<BalanceMovement>();

    public DbSet<AuditRecord> AuditRecords => Set<AuditRecord>();

    IQueryable<VacationRequest> IApplicationDbContext.VacationRequests => VacationRequests;

    IQueryable<VacationBalance> IApplicationDbContext.VacationBalances => VacationBalances;

    IQueryable<BalanceMovement> IApplicationDbContext.BalanceMovements => BalanceMovements;

    IQueryable<AuditRecord> IApplicationDbContext.AuditRecords => AuditRecords;

    public void AddVacationRequest(VacationRequest request)
    {
        VacationRequests.Add(request);
    }

    public void AddVacationBalance(VacationBalance balance)
    {
        VacationBalances.Add(balance);
    }

    public void AddBalanceMovement(BalanceMovement movement)
    {
        BalanceMovements.Add(movement);
    }

    public void AddAuditRecord(AuditRecord auditRecord)
    {
        AuditRecords.Add(auditRecord);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(NovaLeaveDbContext).Assembly);
    }
}
