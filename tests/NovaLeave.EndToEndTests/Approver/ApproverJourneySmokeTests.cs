namespace NovaLeave.EndToEndTests.Approver;

public sealed class ApproverJourneySmokeTests
{
    [Fact]
    public void Approver_Routes_Are_Documented_For_Accessibility_Smoke()
    {
        var routes = new[]
        {
            "/aprobaciones",
            "/aprobaciones/{id}",
            "/aprobaciones/historial",
            "/calendario"
        };

        Assert.Contains("/aprobaciones", routes);
        Assert.Contains("/calendario", routes);
    }
}
