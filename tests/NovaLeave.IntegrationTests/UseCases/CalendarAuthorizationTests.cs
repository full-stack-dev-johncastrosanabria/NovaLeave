using System.Net;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.UseCases;

public sealed class CalendarAuthorizationTests
{
    [Fact]
    public async Task User_Calendar_Renders_Personal_Approved_Period_With_Owner_Detail_Link()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await ApproverTestData.SeedUserAndApproverAsync(factory);
        var client = factory.CreateClient();
        var requestId = await ApproverTestData.CreatePendingRequestAsync(factory, client, "user-1");
        await ApproveAsync(factory, client, requestId);

        var response = await client.SendAsync(IntegrationTestDatabase.AuthenticatedGet("/calendario?context=User", "user-1", "User"));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Mi calendario", html);
        Assert.Contains("role=\"grid\"", html);
        Assert.Contains($"/mis-solicitudes/{requestId}", html);
        Assert.Contains("Solicitud propia", html);
    }

    [Fact]
    public async Task Eligible_Approver_Calendar_Renders_Approved_Periods_Anonymized_Without_Detail_Link()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await ApproverTestData.SeedUserAndApproverAsync(factory);
        var client = factory.CreateClient();
        var requestId = await ApproverTestData.CreatePendingRequestAsync(factory, client, "user-1");
        await ApproveAsync(factory, client, requestId);

        var response = await client.SendAsync(ApproverTestData.ApproverGet("/calendario?context=Approver"));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Calendario de aprobaciones", html);
        Assert.Contains("Evento aprobado", html);
        Assert.Contains("aria-label=", html);
        Assert.DoesNotContain("user1@example.test", html);
        Assert.DoesNotContain($"/aprobaciones/{requestId}", html);
        Assert.DoesNotContain("Vacaciones familiares", html);
    }

    [Fact]
    public async Task HR_Calendar_Renders_Organization_Scope_Only_On_Dedicated_Route()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await ApproverTestData.SeedUserAndApproverAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "hr-1", "hr@example.test", 0, roles: "HR");
        var client = factory.CreateClient();
        var requestId = await ApproverTestData.CreatePendingRequestAsync(factory, client, "user-1");
        await ApproveAsync(factory, client, requestId);

        var response = await client.SendAsync(IntegrationTestDatabase.AuthenticatedGet("/rrhh/calendario", "hr-1", "HR"));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Calendario RRHH", html);
        Assert.Contains("user1@example.test", html);
        Assert.Contains($"/rrhh/solicitudes/{requestId}", html);
        Assert.DoesNotContain("/aprobaciones/", html);
    }

    [Fact]
    public async Task HR_Is_Denied_Shared_Calendar_And_Disabled_Approver_Is_Denied_Approver_Context()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "hr-1", "hr@example.test", 0, roles: "HR");
        await IntegrationTestDatabase.SeedUserAsync(factory, "approver-disabled", "disabled@example.test", 0, roles: "Approver", canResolveRequests: false);
        var client = factory.CreateClient();

        var hrShared = await client.SendAsync(IntegrationTestDatabase.AuthenticatedGet("/calendario", "hr-1", "HR"));
        var disabledApprover = await client.SendAsync(IntegrationTestDatabase.AuthenticatedGet("/calendario?context=Approver", "approver-disabled", "Approver", canResolveRequests: false));

        Assert.Equal(HttpStatusCode.Forbidden, hrShared.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, disabledApprover.StatusCode);
    }

    [Fact]
    public async Task Multi_Role_User_Uses_Explicit_Selected_Calendar_Context()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await ApproverTestData.SeedUserAndApproverAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "multi-1", "multi@example.test", 10, roles: "User,Approver", canResolveRequests: true);
        var client = factory.CreateClient();
        var requestId = await ApproverTestData.CreatePendingRequestAsync(factory, client, "user-1");
        await ApproveAsync(factory, client, requestId);

        var userContext = await client.SendAsync(IntegrationTestDatabase.AuthenticatedGet("/calendario?context=User", "multi-1", "User,Approver", canResolveRequests: true));
        var approverContext = await client.SendAsync(IntegrationTestDatabase.AuthenticatedGet("/calendario?context=Approver", "multi-1", "User,Approver", canResolveRequests: true));
        var userHtml = await userContext.Content.ReadAsStringAsync();
        var approverHtml = await approverContext.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, userContext.StatusCode);
        Assert.Equal(HttpStatusCode.OK, approverContext.StatusCode);
        Assert.Contains("Mi calendario", userHtml);
        Assert.DoesNotContain("Evento aprobado", userHtml);
        Assert.Contains("Calendario de aprobaciones", approverHtml);
        Assert.Contains("Evento aprobado", approverHtml);
    }

    [Fact]
    public async Task User_And_HR_Identity_Can_Use_Explicit_User_Calendar_Without_HR_Scope()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "multi-1", "multi@example.test", 10, roles: "User,HR");
        var client = factory.CreateClient();

        var response = await client.SendAsync(
            IntegrationTestDatabase.AuthenticatedGet("/calendario?context=User", "multi-1", "User,HR"));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Mi calendario", html);
        Assert.DoesNotContain("Calendario RRHH", html);
    }

    [Fact]
    public async Task Triple_Role_Identity_Uses_Explicit_Approver_Context_And_Preserves_It_In_Month_Navigation()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await ApproverTestData.SeedUserAndApproverAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "multi-1", "multi@example.test", 10, roles: "User,Approver,HR", canResolveRequests: true);
        var client = factory.CreateClient();

        var response = await client.SendAsync(IntegrationTestDatabase.AuthenticatedGet(
            "/calendario?context=Approver&year=2027&month=1",
            "multi-1",
            "User,Approver,HR",
            canResolveRequests: true));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Calendario de aprobaciones", html);
        Assert.Contains("context=Approver", html);
        Assert.Contains("year=2026&amp;month=12", html);
        Assert.Contains("year=2027&amp;month=2", html);
        var userContext = await client.SendAsync(IntegrationTestDatabase.AuthenticatedGet(
            "/calendario?context=User",
            "multi-1",
            "User,Approver,HR",
            canResolveRequests: true));
        Assert.Equal(HttpStatusCode.OK, userContext.StatusCode);
    }

    [Fact]
    public async Task Approver_Only_Default_Calendar_Renders_Approver_Context_In_Page_And_Navigation()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await ApproverTestData.SeedUserAndApproverAsync(factory);
        var client = factory.CreateClient();

        var response = await client.SendAsync(ApproverTestData.ApproverGet("/calendario"));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Calendario de aprobaciones", html);
        Assert.Contains("aria-label=\"Contexto activo\">Aprobaciones", html);
        Assert.Contains("href=\"/calendario?context=Approver\"", html);
    }

    [Fact]
    public async Task Inactive_Approver_Is_Denied_Approver_Calendar()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "approver-inactive", "inactive@example.test", 0, isActive: false, roles: "Approver", canResolveRequests: true);
        var client = factory.CreateClient();

        var response = await client.SendAsync(IntegrationTestDatabase.AuthenticatedGet(
            "/calendario?context=Approver",
            "approver-inactive",
            "Approver",
            isActive: false,
            canResolveRequests: true));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static async Task ApproveAsync(NovaLeaveWebApplicationFactory factory, HttpClient client, Guid requestId)
    {
        var rowVersion = ApproverTestData.RowVersionFor(factory, requestId);
        var response = await client.SendAsync(ApproverTestData.ApproverPost($"/aprobaciones/{requestId}/aprobar", ApproverTestData.Form(("RowVersion", rowVersion))));
        if (response.StatusCode != HttpStatusCode.Redirect)
        {
            throw new InvalidOperationException($"Expected approval redirect, got {response.StatusCode}.");
        }
    }
}
