namespace NovaLeave.IntegrationTests.Support;

public sealed class SqlServerFixture
{
    public const string ConnectionStringEnvironmentVariable = "NOVALEAVE_TEST_SQLSERVER";
    private static readonly string DatabaseName = $"NovaLeave_Phase3_{Environment.ProcessId}";

    public static string DefaultConnectionString =>
        Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable) ??
        $"Server=(localdb)\\MSSQLLocalDB;Database={DatabaseName};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

    public string ConnectionString { get; } = DefaultConnectionString;
}
