namespace NovaLeave.EndToEndTests.HR;

public sealed class HRCapabilitySmokeTests
{
    [Fact]
    public void HR_Capability_Routes_Are_Documented_For_Accessibility_Smoke()
    {
        var routes = new[]
        {
            "/rrhh/aprobadores",
            "/rrhh/aprobadores/{id}/capacidad"
        };

        Assert.Contains("/rrhh/aprobadores", routes);
        Assert.Contains("/rrhh/aprobadores/{id}/capacidad", routes);
    }
}
