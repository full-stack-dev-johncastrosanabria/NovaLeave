using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using NovaLeave.Web.Controllers;
using NovaLeave.IntegrationTests.Support;
using NovaLeave.IntegrationTests.UseCases;

namespace NovaLeave.IntegrationTests.Traceability;

public sealed class UseCaseRouteTraceabilityTests
{
    [Theory]
    [InlineData("RequireActiveUser")]
    [InlineData("RequireActiveApprover")]
    [InlineData("RequireActiveHR")]
    public async Task Required_Authorization_Policies_Are_Registered(string policyName)
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        var provider = factory.Services.GetRequiredService<IAuthorizationPolicyProvider>();

        var policy = await provider.GetPolicyAsync(policyName);

        Assert.NotNull(policy);
    }

    [Fact]
    public async Task UC01_Through_UC22_Routes_Are_Reachable_Only_In_Authorized_Role_Context()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await ApproverTestData.SeedUserAndApproverAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "hr-1", "hr@example.test", 0, roles: "HR");
        var client = factory.CreateClient();
        var requestId = await ApproverTestData.CreatePendingRequestAsync(factory, client, "user-1");

        var checks = new[]
        {
            RouteCheck.Get("/Identity/Account/Login", null, HttpStatusCode.OK),
            RouteCheck.Get("/mis-solicitudes", User(), HttpStatusCode.OK),
            RouteCheck.Get("/mis-solicitudes/crear", User(), HttpStatusCode.OK),
            RouteCheck.Get($"/mis-solicitudes/{requestId}", User(), HttpStatusCode.OK),
            RouteCheck.Get($"/mis-solicitudes/{requestId}/editar", User(), HttpStatusCode.OK),
            RouteCheck.Get("/saldo", User(), HttpStatusCode.OK),
            RouteCheck.Get("/calendario?context=User", User(), HttpStatusCode.OK),
            RouteCheck.Get("/aprobaciones", Approver(), HttpStatusCode.OK),
            RouteCheck.Get($"/aprobaciones/{requestId}", Approver(), HttpStatusCode.OK),
            RouteCheck.Get("/aprobaciones/historial", Approver(), HttpStatusCode.OK),
            RouteCheck.Get("/calendario?context=Approver", Approver(), HttpStatusCode.OK),
            RouteCheck.Get("/rrhh", HR(), HttpStatusCode.OK),
            RouteCheck.Get("/rrhh/solicitudes", HR(), HttpStatusCode.OK),
            RouteCheck.Get($"/rrhh/solicitudes/{requestId}", HR(), HttpStatusCode.OK),
            RouteCheck.Get("/rrhh/calendario", HR(), HttpStatusCode.OK),
            RouteCheck.Get("/rrhh/saldos", HR(), HttpStatusCode.OK),
            RouteCheck.Get("/rrhh/saldos/user-1", HR(), HttpStatusCode.OK),
            RouteCheck.Get("/rrhh/auditoria", HR(), HttpStatusCode.OK),
            RouteCheck.Get("/rrhh/aprobadores", HR(), HttpStatusCode.OK),
            RouteCheck.Get("/rrhh/aprobadores/approver-1/capacidad", HR(), HttpStatusCode.OK),
            RouteCheck.Get("/rrhh/solicitudes", User(), HttpStatusCode.Forbidden),
            RouteCheck.Get("/aprobaciones", User(), HttpStatusCode.Forbidden),
            RouteCheck.Get("/mis-solicitudes", HR(), HttpStatusCode.Forbidden),
            RouteCheck.Get("/calendario", HR(), HttpStatusCode.Forbidden)
        };

        foreach (var check in checks)
        {
            var response = await client.SendAsync(check.CreateRequest());
            Assert.Equal(check.ExpectedStatus, response.StatusCode);
        }
    }

    [Fact]
    public void UC01_Through_UC22_Mvc_Route_Templates_Are_Declared_And_Background_UCs_Have_No_Http_Route()
    {
        var routeTemplates = new HashSet<string>(
            typeof(MisSolicitudesController).Assembly
                .GetTypes()
                .Where(type => typeof(Controller).IsAssignableFrom(type))
                .SelectMany(type => type.GetMethods())
                .SelectMany(method => method.GetCustomAttributes(typeof(HttpMethodAttribute), inherit: false).Cast<HttpMethodAttribute>())
                .SelectMany(attribute => attribute.Template is null ? [] : new[] { attribute.Template }),
            StringComparer.OrdinalIgnoreCase);

        var expectedHttpRoutes = new[]
        {
            "/mis-solicitudes",
            "/mis-solicitudes/crear",
            "/mis-solicitudes/{id:guid}",
            "/mis-solicitudes/{id:guid}/editar",
            "/saldo",
            "/calendario",
            "/aprobaciones",
            "/aprobaciones/{id:guid}",
            "/aprobaciones/{id:guid}/aprobar",
            "/aprobaciones/{id:guid}/rechazar",
            "/aprobaciones/{id:guid}/desactivar",
            "/aprobaciones/historial",
            "/rrhh",
            "/rrhh/solicitudes",
            "/rrhh/solicitudes/{id:guid}",
            "/rrhh/calendario",
            "/rrhh/saldos",
            "/rrhh/saldos/{userId}",
            "/rrhh/auditoria",
            "/rrhh/aprobadores",
            "/rrhh/aprobadores/{id}/capacidad"
        };

        foreach (var route in expectedHttpRoutes)
        {
            Assert.Contains(route, routeTemplates);
        }

        Assert.DoesNotContain(routeTemplates, route => route.Contains("timeout", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(routeTemplates, route => route.Contains("accrual", StringComparison.OrdinalIgnoreCase));
    }

    private static AuthContext User() => new("user-1", "User", true, false);

    private static AuthContext Approver() => new("approver-1", "Approver", true, true);

    private static AuthContext HR() => new("hr-1", "HR", true, false);

    private sealed record AuthContext(string UserId, string Roles, bool IsActive, bool CanResolveRequests);

    private sealed record RouteCheck(string Path, AuthContext? Auth, HttpStatusCode ExpectedStatus)
    {
        public static RouteCheck Get(string path, AuthContext? auth, HttpStatusCode expectedStatus)
        {
            return new RouteCheck(path, auth, expectedStatus);
        }

        public HttpRequestMessage CreateRequest()
        {
            return Auth is null
                ? new HttpRequestMessage(HttpMethod.Get, Path)
                : IntegrationTestDatabase.AuthenticatedGet(Path, Auth.UserId, Auth.Roles, Auth.IsActive, Auth.CanResolveRequests);
        }
    }
}
