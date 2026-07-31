using System.Net;
using Microsoft.Extensions.DependencyInjection;
using NovaLeave.Domain.Enums;
using NovaLeave.Infrastructure.Persistence;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.UseCases;

public sealed class UC12RejectRequestTests
{
    [Fact]
    public async Task Reject_Requires_Normalized_Reason_Releases_Reservation_And_Audits()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await ApproverTestData.SeedUserAndApproverAsync(factory);
        var client = factory.CreateClient();
        var requestId = await ApproverTestData.CreatePendingRequestAsync(factory, client);
        var rowVersion = ApproverTestData.RowVersionFor(factory, requestId);

        var invalid = await client.SendAsync(ApproverTestData.ApproverPost($"/aprobaciones/{requestId}/rechazar", ApproverTestData.Form(("RowVersion", rowVersion), ("RejectionReason", " corto "))));
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);

        var valid = await client.SendAsync(ApproverTestData.ApproverPost($"/aprobaciones/{requestId}/rechazar", ApproverTestData.Form(("RowVersion", rowVersion), ("RejectionReason", "  No cumple con la politica interna.  "))));
        Assert.Equal(HttpStatusCode.Redirect, valid.StatusCode);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        var request = db.VacationRequests.Single(request => request.Id == requestId);
        var balance = db.VacationBalances.Single(balance => balance.UserId == "user-1");
        Assert.Equal(RequestStatus.Rejected, request.Status);
        Assert.Equal("No cumple con la politica interna.", request.RejectionReason);
        Assert.Equal(0, balance.ReservedDays);
        Assert.Single(db.BalanceMovements.Where(movement => movement.RequestId == requestId && movement.Type == MovementType.Release));
        Assert.Contains(db.AuditRecords, audit => audit.Action == "Reject" && audit.EntityId == requestId && (audit.Data == null || !audit.Data.Contains("RejectionReason", StringComparison.OrdinalIgnoreCase)));
    }
}
