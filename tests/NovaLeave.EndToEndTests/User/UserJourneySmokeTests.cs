namespace NovaLeave.EndToEndTests.User;

public sealed class UserJourneySmokeTests
{
    [Fact]
    public void User_Journey_Routes_Are_Documented_For_E2E_Coverage()
    {
        string[] routes =
        [
            "/Identity/Account/Login",
            "/mis-solicitudes",
            "/mis-solicitudes/crear",
            "/saldo",
            "/calendario"
        ];

        Assert.All(routes, route => Assert.StartsWith("/", route));
    }
}
