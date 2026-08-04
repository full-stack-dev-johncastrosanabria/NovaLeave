using System.Net;
using Microsoft.Extensions.DependencyInjection;
using NovaLeave.Domain.Entities;
using NovaLeave.Infrastructure.Persistence;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.UseCases;

public sealed class UC21HRAuditTests
{
    [Fact]
    public async Task HR_Audit_Log_Shows_Relevant_Redacted_Audit_Records()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        var requestId = await SeedRequestAndSensitiveAccessAsync(factory);
        var client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
            Assert.Contains(db.AuditRecords, record => record.Action == "HRSensitiveAccess" && record.EntityId == requestId);
        }

        var response = await client.SendAsync(HRGet("/rrhh/auditoria"));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Auditoría RRHH", html);
        Assert.Contains("HRSensitiveAccess", html);
        Assert.Contains(requestId.ToString(), html);
        Assert.DoesNotContain("Vacaciones familiares de inicio de ano.", html);
    }

    [Fact]
    public async Task HR_Audit_Log_Filters_By_Action_Query_Parameter()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "hr-1", "hr@example.test", 0, roles: "HR");
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
            var entityId = Guid.NewGuid();
            db.AuditRecords.Add(AuditRecord.Create("system", "System", "VisibleAction", "VacationRequest", entityId, "Success", Guid.NewGuid(), entityId, null, DateTimeOffset.UtcNow));
            db.AuditRecords.Add(AuditRecord.Create("system", "System", "HiddenAction", "VacationRequest", Guid.NewGuid(), "Success", Guid.NewGuid(), Guid.NewGuid(), null, DateTimeOffset.UtcNow));
            await db.SaveChangesAsync();
        }

        var client = factory.CreateClient();
        var response = await client.SendAsync(HRGet("/rrhh/auditoria?action=VisibleAction"));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("VisibleAction", html);
        Assert.DoesNotContain("HiddenAction", html);
    }

    [Fact]
    public async Task HR_Request_Detail_Audits_Sensitive_Reason_Access_Without_Reason_Content()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await ApproverTestData.SeedUserAndApproverAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "hr-1", "hr@example.test", 0, roles: "HR");
        var client = factory.CreateClient();
        var requestId = await ApproverTestData.CreatePendingRequestAsync(factory, client, "user-1");

        var response = await client.SendAsync(HRGet($"/rrhh/solicitudes/{requestId}"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        var audit = db.AuditRecords.Single(record => record.Action == "HRSensitiveAccess" && record.EntityId == requestId);
        Assert.Equal("hr-1", audit.ActorId);
        Assert.Contains("Reason", audit.Data);
        Assert.DoesNotContain("Vacaciones familiares", audit.Data);
        Assert.DoesNotContain("RejectionReason", audit.Data);
    }

    [Fact]
    public async Task HR_Audit_Log_Uses_Approved_Fifty_Row_Pagination()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "hr-1", "hr@example.test", 0, roles: "HR");
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
            for (var index = 1; index <= 51; index++)
            {
                var entityId = Guid.Parse($"00000000-0000-0000-0000-{index:000000000000}");
                db.AuditRecords.Add(AuditRecord.Create(
                    "system",
                    "System",
                    $"AuditPage-{index:00}",
                    "VacationRequest",
                    entityId,
                    "Success",
                    Guid.NewGuid(),
                    entityId,
                    null,
                    DateTimeOffset.UtcNow.AddMinutes(index)));
            }

            await db.SaveChangesAsync();
        }

        var client = factory.CreateClient();
        var response = await client.SendAsync(HRGet("/rrhh/auditoria?page=2&pageSize=50"));
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Pagina 2 de 2", html);
        Assert.Contains("AuditPage-01", html);
    }

    private static async Task<Guid> SeedRequestAndSensitiveAccessAsync(NovaLeaveWebApplicationFactory factory)
    {
        await ApproverTestData.SeedUserAndApproverAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "hr-1", "hr@example.test", 0, roles: "HR");
        var client = factory.CreateClient();
        var requestId = await ApproverTestData.CreatePendingRequestAsync(factory, client, "user-1");
        var detailResponse = await client.SendAsync(HRGet($"/rrhh/solicitudes/{requestId}"));
        if (detailResponse.StatusCode != HttpStatusCode.OK)
        {
            throw new InvalidOperationException($"Expected HR detail OK, got {detailResponse.StatusCode}.");
        }

        return requestId;
    }

    private static HttpRequestMessage HRGet(string path)
    {
        return IntegrationTestDatabase.AuthenticatedGet(path, "hr-1", "HR");
    }
}
