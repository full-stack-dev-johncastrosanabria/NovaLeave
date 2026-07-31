namespace NovaLeave.IntegrationTests.Support;

public sealed class SqlServerFixture
{
    public const string ConnectionStringEnvironmentVariable = "NOVALEAVE_TEST_SQLSERVER";

    public static string DefaultConnectionString =>
        Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable) ??
        "Server=(localdb)\\MSSQLLocalDB;Database=NovaLeave_Phase2_Test;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

    public string ConnectionString { get; } = DefaultConnectionString;
}
