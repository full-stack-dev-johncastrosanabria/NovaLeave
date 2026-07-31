using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NovaLeave.Infrastructure.Persistence;

public sealed class NovaLeaveDbContextFactory : IDesignTimeDbContextFactory<NovaLeaveDbContext>
{
    public NovaLeaveDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("NOVALEAVE_DESIGNTIME_SQLSERVER") ??
            "Server=(localdb)\\MSSQLLocalDB;Database=NovaLeave_DesignTime;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

        var options = new DbContextOptionsBuilder<NovaLeaveDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new NovaLeaveDbContext(options);
    }
}
