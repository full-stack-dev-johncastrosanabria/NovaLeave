using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NovaLeave.Infrastructure.Persistence;

namespace NovaLeave.Infrastructure.HealthChecks;

public sealed class DatabaseConnectivityHealthCheck : IHealthCheck
{
    private readonly NovaLeaveDbContext _dbContext;

    public DatabaseConnectivityHealthCheck(NovaLeaveDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Database.CanConnectAsync(cancellationToken)
            ? HealthCheckResult.Healthy("database reachable")
            : HealthCheckResult.Unhealthy("database unreachable");
    }
}
