using System.Net;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.UseCases;

public sealed class UC01AuthenticateTests
{
    [Fact]
    public async Task Login_Page_Is_Available_Anonymously()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/Identity/Account/Login");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Iniciar sesion", html);
    }
}
