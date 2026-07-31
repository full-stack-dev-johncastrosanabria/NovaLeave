using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NovaLeave.Infrastructure.Persistence;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.Persistence;

public sealed class InitialMigrationValidationTests
{
    [Fact]
    public async Task Initial_Migration_Applies_To_Clean_SqlServer_Database_And_Creates_Expected_Model()
    {
        var options = new DbContextOptionsBuilder<NovaLeaveDbContext>()
            .UseSqlServer(SqlServerFixture.DefaultConnectionString)
            .Options;

        await using var context = new NovaLeaveDbContext(options);

        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();

        var tables = await QueryStringsAsync(context, "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'");
        Assert.Contains("VacationRequest", tables);
        Assert.Contains("VacationBalance", tables);
        Assert.Contains("BalanceMovement", tables);
        Assert.Contains("AuditRecord", tables);
        Assert.Contains("AspNetUsers", tables);
        Assert.DoesNotContain("LeaveType", tables);
        Assert.DoesNotContain("SystemParameter", tables);
        Assert.DoesNotContain("SecurityEvent", tables);
        Assert.DoesNotContain("OutboxMessage", tables);

        var rowVersionColumns = await QueryStringsAsync(context, """
            SELECT TABLE_NAME + '.' + COLUMN_NAME
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE DATA_TYPE = 'timestamp'
            """);
        Assert.Contains("VacationRequest.RowVersion", rowVersionColumns);
        Assert.Contains("VacationBalance.RowVersion", rowVersionColumns);
        Assert.Contains("AspNetUsers.RowVersion", rowVersionColumns);

        var foreignKeys = await QueryStringsAsync(context, """
            SELECT name
            FROM sys.foreign_keys
            WHERE name IN (
                'FK_VacationRequest_AspNetUsers_OwnerId',
                'FK_VacationBalance_AspNetUsers_UserId',
                'FK_BalanceMovement_VacationBalance_BalanceId',
                'FK_BalanceMovement_VacationRequest_RequestId')
            """);
        Assert.Equal(4, foreignKeys.Count);

        var indexes = await QueryStringsAsync(context, """
            SELECT name
            FROM sys.indexes
            WHERE name IN (
                'UX_VacationBalance_UserId',
                'UX_VacationRequest_Owner_DateRange_Status',
                'UX_BalanceMovement_Accrual')
            """);
        Assert.Equal(3, indexes.Count);
    }

    private static async Task<List<string>> QueryStringsAsync(NovaLeaveDbContext context, string sql)
    {
        var results = new List<string>();
        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = sql;

        if (command.Connection!.State != System.Data.ConnectionState.Open)
        {
            await command.Connection.OpenAsync();
        }

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(reader.GetString(0));
        }

        return results;
    }
}
