using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NovaLeave.Application.Approvals.DeactivateApprovedRequest;
using NovaLeave.Domain.Enums;
using NovaLeave.Infrastructure.Persistence;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.UseCases;

public sealed class UC13DeactivateApprovedRequestTests
{
    [Fact]
    public async Task Deactivate_PreStart_Approved_Request_Restores_Deduction_And_Audits_Atomically()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await ApproverTestData.SeedUserAndApproverAsync(factory);
        var client = factory.CreateClient();
        var requestId = await CreateApprovedRequestAsync(factory, client);
        var rowVersion = ApproverTestData.RowVersionFor(factory, requestId);

        var response = await client.SendAsync(ApproverTestData.ApproverPost(
            $"/aprobaciones/{requestId}/desactivar",
            ApproverTestData.Form(("RowVersion", rowVersion))));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        var request = db.VacationRequests.Single(request => request.Id == requestId);
        var balance = db.VacationBalances.Single(balance => balance.UserId == "user-1");
        Assert.Equal(RequestStatus.CancelledByApprover, request.Status);
        Assert.Equal(0, balance.ReservedDays);
        Assert.Equal(0, balance.DeductedDays);
        Assert.Equal(10, balance.AvailableDays);
        Assert.Single(db.BalanceMovements.Where(movement => movement.RequestId == requestId && movement.Type == MovementType.Restoration));
        Assert.Single(db.AuditRecords.Where(audit => audit.EntityId == requestId && audit.Action == "Deactivate" && audit.ActorId == "approver-1"));
    }

    [Fact]
    public async Task Deactivate_Command_Revalidates_RowVersion_And_Approver_Capability()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await ApproverTestData.SeedUserAndApproverAsync(factory);
        var client = factory.CreateClient();
        var requestId = await CreateApprovedRequestAsync(factory, client);
        await SetApproverCapabilityAsync(factory, "approver-1", canResolve: false);

        using (var disabledScope = factory.Services.CreateScope())
        {
            var handler = disabledScope.ServiceProvider.GetRequiredService<DeactivateApprovedRequestHandler>();
            var disabled = await handler.HandleAsync(
                new DeactivateApprovedRequestCommand("approver-1", requestId, ApproverTestData.RowVersionBytesFor(factory, requestId)),
                CancellationToken.None);
            Assert.True(disabled.IsFailure);
        }

        await SetApproverCapabilityAsync(factory, "approver-1", canResolve: true);
        using (var staleScope = factory.Services.CreateScope())
        {
            var handler = staleScope.ServiceProvider.GetRequiredService<DeactivateApprovedRequestHandler>();
            var stale = await handler.HandleAsync(
                new DeactivateApprovedRequestCommand("approver-1", requestId, [1, 2, 3]),
                CancellationToken.None);
            Assert.True(stale.IsFailure);
        }

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        Assert.Equal(RequestStatus.Approved, db.VacationRequests.Single(request => request.Id == requestId).Status);
        Assert.Single(db.BalanceMovements.Where(movement => movement.RequestId == requestId && movement.Type == MovementType.Deduction));
        Assert.DoesNotContain(db.BalanceMovements, movement => movement.RequestId == requestId && movement.Type == MovementType.Restoration);
    }

    internal static async Task<Guid> CreateApprovedRequestAsync(
        NovaLeaveWebApplicationFactory factory,
        HttpClient client,
        string ownerId = "user-1",
        bool approvalCanResolve = true)
    {
        var requestId = await ApproverTestData.CreatePendingRequestAsync(factory, client, ownerId);
        var rowVersion = ApproverTestData.RowVersionFor(factory, requestId);
        var response = await client.SendAsync(ApproverTestData.ApproverPost(
            $"/aprobaciones/{requestId}/aprobar",
            ApproverTestData.Form(("RowVersion", rowVersion)),
            canResolve: approvalCanResolve));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        return requestId;
    }

    internal static async Task SetApproverCapabilityAsync(NovaLeaveWebApplicationFactory factory, string userId, bool canResolve)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE AspNetUsers SET CanResolveRequests = {canResolve} WHERE Id = {userId}");
    }
}
