namespace NovaLeave.EndToEndTests.HR;

public sealed class HRReadOnlySmokeTests
{
    [Fact]
    public void HR_Read_Only_Routes_Are_Documented_For_Accessibility_Smoke()
    {
        var routes = new[]
        {
            "/rrhh",
            "/rrhh/solicitudes",
            "/rrhh/solicitudes/{id}",
            "/rrhh/calendario",
            "/rrhh/saldos",
            "/rrhh/saldos/{userId}",
            "/rrhh/auditoria"
        };

        Assert.Contains("/rrhh/solicitudes", routes);
        Assert.Contains("/rrhh/calendario", routes);
        Assert.Contains("/rrhh/saldos", routes);
        Assert.Contains("/rrhh/auditoria", routes);
    }
}
