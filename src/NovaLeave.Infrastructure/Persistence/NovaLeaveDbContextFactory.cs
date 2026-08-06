using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NovaLeave.Infrastructure.Persistence;

public sealed class NovaLeaveDbContextFactory : IDesignTimeDbContextFactory<NovaLeaveDbContext>
{
    public NovaLeaveDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("NOVALEAVE_DESIGNTIME_SQLSERVER") ??
            "Server=localhost,1433;Database=NovaLeave;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;";

        var options = new DbContextOptionsBuilder<NovaLeaveDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new NovaLeaveDbContext(options);
    }
}
