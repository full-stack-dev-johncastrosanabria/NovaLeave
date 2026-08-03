using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NovaLeave.Application.System.CancelTimedOutRequests;
using NovaLeave.Infrastructure.Persistence;
using NovaLeave.IntegrationTests.Support;
using NovaLeave.IntegrationTests.UseCases;

namespace NovaLeave.IntegrationTests.Audit;

public sealed class AuditCompletenessTests
{
    [Fact]
    public async Task Successful_Request_Lifecycle_Operations_Create_Required_Audit_Records()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await ApproverTestData.SeedUserAndApproverAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "user-2", "user2@example.test", 10);
        await IntegrationTestDatabase.SeedUserAsync(factory, "user-3", "user3@example.test", 10);
        await IntegrationTestDatabase.SeedUserAsync(factory, "user-4", "user4@example.test", 10);
        var client = factory.CreateClient();

        var editedRequestId = await ApproverTestData.CreatePendingRequestAsync(factory, client, "user-1");
        await EditPendingRequestAsync(factory, client, editedRequestId);

        var approvedRequestId = await ApproverTestData.CreatePendingRequestAsync(factory, client, "user-2");
        await ApproveRequestAsync(factory, client, approvedRequestId);

        var rejectedRequestId = await ApproverTestData.CreatePendingRequestAsync(factory, client, "user-3");
        await RejectRequestAsync(factory, client, rejectedRequestId);

        var timedOutRequestId = await ApproverTestData.CreatePendingRequestAsync(factory, client, "user-4");
        await UC16TimeoutCancellationTests.AgeRequestAsync(factory, timedOutRequestId, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        using (var timeoutScope = factory.Services.CreateScope())
        {
            var handler = timeoutScope.ServiceProvider.GetRequiredService<CancelTimedOutRequestsHandler>();
            var result = await handler.HandleAsync(new CancelTimedOutRequestsCommand(new DateOnly(2026, 1, 15)), CancellationToken.None);
            Assert.True(result.IsSuccess);
        }

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        AssertAudit(db, editedRequestId, "Create", "user-1", "User", "Success");
        AssertAudit(db, editedRequestId, "EditPending", "user-1", "User", "Success");
        AssertAudit(db, approvedRequestId, "Create", "user-2", "User", "Success");
        AssertAudit(db, approvedRequestId, "Approve", "approver-1", "Approver", "Success");
        AssertAudit(db, rejectedRequestId, "Create", "user-3", "User", "Success");
        AssertAudit(db, rejectedRequestId, "Reject", "approver-1", "Approver", "Success");
        AssertAudit(db, timedOutRequestId, "Create", "user-4", "User", "Success");
        AssertAudit(db, timedOutRequestId, "Timeout", "System", "System", "Success");
    }

    [Fact]
    public async Task PreStart_Deactivation_Creates_Deactivation_Audit_Record()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await ApproverTestData.SeedUserAndApproverAsync(factory);
        var client = factory.CreateClient();
        var requestId = await UC13DeactivateApprovedRequestTests.CreateApprovedRequestAsync(factory, client);
        var rowVersion = ApproverTestData.RowVersionFor(factory, requestId);

        var response = await client.SendAsync(ApproverTestData.ApproverPost(
            $"/aprobaciones/{requestId}/desactivar",
            ApproverTestData.Form(("RowVersion", rowVersion))));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        AssertAudit(db, requestId, "Deactivate", "approver-1", "Approver", "Success");
    }

    private static async Task EditPendingRequestAsync(NovaLeaveWebApplicationFactory factory, HttpClient client, Guid requestId)
    {
        var rowVersion = ApproverTestData.RowVersionFor(factory, requestId);
        var response = await client.SendAsync(IntegrationTestDatabase.AuthenticatedPost(
            $"/mis-solicitudes/{requestId}/editar",
            ApproverTestData.Form(
                ("InputMode", "dateRange"),
                ("StartDate", "2027-01-11"),
                ("EndDate", "2027-01-13"),
                ("Reason", "Vacaciones familiares reprogramadas."),
                ("RowVersion", rowVersion)),
            "user-1",
            "User"));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    private static async Task ApproveRequestAsync(NovaLeaveWebApplicationFactory factory, HttpClient client, Guid requestId)
    {
        var response = await client.SendAsync(ApproverTestData.ApproverPost(
            $"/aprobaciones/{requestId}/aprobar",
            ApproverTestData.Form(("RowVersion", ApproverTestData.RowVersionFor(factory, requestId)))));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    private static async Task RejectRequestAsync(NovaLeaveWebApplicationFactory factory, HttpClient client, Guid requestId)
    {
        var response = await client.SendAsync(ApproverTestData.ApproverPost(
            $"/aprobaciones/{requestId}/rechazar",
            ApproverTestData.Form(
                ("RowVersion", ApproverTestData.RowVersionFor(factory, requestId)),
                ("RejectionReason", "Solicitud rechazada por regla valida."))));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    private static void AssertAudit(NovaLeaveDbContext db, Guid requestId, string action, string actorId, string actorRole, string result)
    {
        var audit = db.AuditRecords.Single(record => record.EntityId == requestId && record.Action == action);
        Assert.Equal(actorId, audit.ActorId);
        Assert.Equal(actorRole, audit.ActorRole);
        Assert.Equal(result, audit.Result);
        Assert.NotEqual(Guid.Empty, audit.CorrelationId);
        Assert.NotEqual(Guid.Empty, audit.RequestId);
        Assert.DoesNotContain("Reason", audit.Data ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("RejectionReason", audit.Data ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }
}
