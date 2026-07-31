using System.Net;
using Microsoft.Extensions.DependencyInjection;
using NovaLeave.Domain.Enums;
using NovaLeave.Infrastructure.Persistence;
using NovaLeave.IntegrationTests.Support;
using NovaLeave.IntegrationTests.UseCases;

namespace NovaLeave.IntegrationTests.Concurrency;

public sealed class DeactivationConcurrencyTests
{
    [Fact]
    public async Task Duplicate_Deactivation_Race_Has_One_Winner_And_One_Restoration()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await ApproverTestData.SeedUserAndApproverAsync(factory);
        var client = factory.CreateClient();
        var requestId = await UC13DeactivateApprovedRequestTests.CreateApprovedRequestAsync(factory, client);
        var rowVersion = ApproverTestData.RowVersionFor(factory, requestId);

        var first = client.SendAsync(ApproverTestData.ApproverPost(
            $"/aprobaciones/{requestId}/desactivar",
            ApproverTestData.Form(("RowVersion", rowVersion))));
        var second = client.SendAsync(ApproverTestData.ApproverPost(
            $"/aprobaciones/{requestId}/desactivar",
            ApproverTestData.Form(("RowVersion", rowVersion))));
        var responses = await Task.WhenAll(first, second);

        Assert.Contains(responses, response => response.StatusCode == HttpStatusCode.Redirect);
        Assert.Contains(responses, response => response.StatusCode == HttpStatusCode.Conflict);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        Assert.Equal(RequestStatus.CancelledByApprover, db.VacationRequests.Single(request => request.Id == requestId).Status);
        Assert.Single(db.BalanceMovements.Where(movement => movement.RequestId == requestId && movement.Type == MovementType.Restoration));
        Assert.Single(db.AuditRecords.Where(audit => audit.EntityId == requestId && audit.Action == "Deactivate"));
    }
}
