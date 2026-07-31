using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NovaLeave.Application.Audit;
using NovaLeave.Application.System.CancelTimedOutRequests;
using NovaLeave.Domain.Enums;
using NovaLeave.Infrastructure.Persistence;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.UseCases;

public sealed class UC16TimeoutCancellationTests
{
    [Fact]
    public async Task Timeout_Cancels_Eligible_Pending_Request_Releases_Reservation_And_Audits_System_Rule()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await ApproverTestData.SeedUserAndApproverAsync(factory);
        var client = factory.CreateClient();
        var requestId = await ApproverTestData.CreatePendingRequestAsync(factory, client);
        await AgeRequestAsync(factory, requestId, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        using var scope = factory.Services.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<CancelTimedOutRequestsHandler>();

        var result = await handler.HandleAsync(new CancelTimedOutRequestsCommand(new DateOnly(2026, 1, 15)), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        var request = db.VacationRequests.Single(request => request.Id == requestId);
        var balance = db.VacationBalances.Single(balance => balance.UserId == "user-1");
        Assert.Equal(RequestStatus.CancelledByTimeout, request.Status);
        Assert.Equal(0, balance.ReservedDays);
        Assert.Equal(0, balance.DeductedDays);
        Assert.Single(db.BalanceMovements.Where(movement => movement.RequestId == requestId && movement.Type == MovementType.Release));
        var audits = db.AuditRecords.AsEnumerable().ToList();
        Assert.Single(audits, audit =>
            audit.EntityId == requestId &&
            audit.ActorId == SystemAuditWriter.ActorId &&
            audit.ActorRole == SystemAuditWriter.ActorRole &&
            audit.Action == "Timeout" &&
            audit.Data != null &&
            audit.Data.Contains("\"PendingRequestTimeoutDays\":14", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Timeout_Skips_Not_Yet_Eligible_And_NonPending_Requests()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await ApproverTestData.SeedUserAndApproverAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "user-2", "user2@example.test", 10);
        var client = factory.CreateClient();
        var freshRequestId = await ApproverTestData.CreatePendingRequestAsync(factory, client);
        var approvedRequestId = await CreateApprovedOldRequestAsync(factory, client, "user-2");

        using var scope = factory.Services.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<CancelTimedOutRequestsHandler>();

        var result = await handler.HandleAsync(new CancelTimedOutRequestsCommand(new DateOnly(2026, 1, 15)), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        Assert.Equal(RequestStatus.Pending, db.VacationRequests.Single(request => request.Id == freshRequestId).Status);
        Assert.Equal(RequestStatus.Approved, db.VacationRequests.Single(request => request.Id == approvedRequestId).Status);
        Assert.DoesNotContain(db.AuditRecords, audit => audit.Action == "Timeout");
    }

    internal static async Task AgeRequestAsync(NovaLeaveWebApplicationFactory factory, Guid requestId, DateTime createdAtUtc)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE VacationRequest SET CreatedAtUtc = {createdAtUtc}, UpdatedAtUtc = {createdAtUtc} WHERE Id = {requestId}");
    }

    private static async Task<Guid> CreateApprovedOldRequestAsync(NovaLeaveWebApplicationFactory factory, HttpClient client, string userId)
    {
        var requestId = await ApproverTestData.CreatePendingRequestAsync(factory, client, userId);
        var rowVersion = ApproverTestData.RowVersionFor(factory, requestId);
        var response = await client.SendAsync(ApproverTestData.ApproverPost(
            $"/aprobaciones/{requestId}/aprobar",
            ApproverTestData.Form(("RowVersion", rowVersion))));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        await AgeRequestAsync(factory, requestId, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        return requestId;
    }
}
