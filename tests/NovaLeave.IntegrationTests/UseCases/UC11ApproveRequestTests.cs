using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NovaLeave.Domain.Enums;
using NovaLeave.Infrastructure.Persistence;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.UseCases;

public sealed class UC11ApproveRequestTests
{
    [Fact]
    public async Task Approve_Converts_Reservation_To_Deduction_And_Audits_Atomically()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await ApproverTestData.SeedUserAndApproverAsync(factory);
        var client = factory.CreateClient();
        var requestId = await ApproverTestData.CreatePendingRequestAsync(factory, client);
        var rowVersion = ApproverTestData.RowVersionFor(factory, requestId);

        var response = await client.SendAsync(ApproverTestData.ApproverPost($"/aprobaciones/{requestId}/aprobar", ApproverTestData.Form(("RowVersion", rowVersion))));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        var request = db.VacationRequests.Single(request => request.Id == requestId);
        var balance = db.VacationBalances.Single(balance => balance.UserId == "user-1");
        Assert.Equal(RequestStatus.Approved, request.Status);
        Assert.Equal(0, balance.ReservedDays);
        Assert.Equal(3, balance.DeductedDays);
        Assert.Single(db.BalanceMovements.Where(movement => movement.RequestId == requestId && movement.Type == MovementType.Deduction));
        Assert.Contains(db.AuditRecords, audit => audit.Action == "Approve" && audit.EntityId == requestId);
    }

    [Fact]
    public async Task Approve_Revalidates_RowVersion_Self_Resolution_And_Negative_Projected_Balance()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await ApproverTestData.SeedUserAndApproverAsync(factory);
        var client = factory.CreateClient();
        var requestId = await ApproverTestData.CreatePendingRequestAsync(factory, client);

        var stale = await client.SendAsync(ApproverTestData.ApproverPost($"/aprobaciones/{requestId}/aprobar", ApproverTestData.Form(("RowVersion", Convert.ToBase64String([1, 2, 3])))));
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);

        await using var selfFactory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(selfFactory);
        await IntegrationTestDatabase.SeedUserAsync(selfFactory, "combo-1", "combo@example.test", 3, roles: "User,Approver", canResolveRequests: true);
        var selfClient = selfFactory.CreateClient();
        var selfRequestId = await ApproverTestData.CreatePendingRequestAsync(selfFactory, selfClient, "combo-1");
        var selfRowVersion = ApproverTestData.RowVersionFor(selfFactory, selfRequestId);
        var self = await selfClient.SendAsync(ApproverTestData.ApproverPost($"/aprobaciones/{selfRequestId}/aprobar", ApproverTestData.Form(("RowVersion", selfRowVersion)), "combo-1"));
        Assert.Equal(HttpStatusCode.NotFound, self.StatusCode);

        await using var insufficientFactory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(insufficientFactory);
        await IntegrationTestDatabase.SeedUserAsync(insufficientFactory, "user-1", "user1@example.test", 3);
        await IntegrationTestDatabase.SeedUserAsync(insufficientFactory, "approver-1", "approver@example.test", 0, roles: "Approver", canResolveRequests: true);
        var insufficientClient = insufficientFactory.CreateClient();
        var insufficientRequestId = await ApproverTestData.CreatePendingRequestAsync(insufficientFactory, insufficientClient);
        var insufficientRowVersion = ApproverTestData.RowVersionFor(insufficientFactory, insufficientRequestId);
        using (var scope = insufficientFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
            await db.Database.ExecuteSqlRawAsync("UPDATE VacationBalance SET AccruedDays = 2 WHERE UserId = 'user-1'");
        }

        var insufficient = await insufficientClient.SendAsync(ApproverTestData.ApproverPost($"/aprobaciones/{insufficientRequestId}/aprobar", ApproverTestData.Form(("RowVersion", insufficientRowVersion))));
        Assert.Equal(HttpStatusCode.Conflict, insufficient.StatusCode);
    }
}
