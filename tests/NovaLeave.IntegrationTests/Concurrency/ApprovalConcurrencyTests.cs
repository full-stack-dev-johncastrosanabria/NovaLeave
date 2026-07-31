using System.Net;
using Microsoft.Extensions.DependencyInjection;
using NovaLeave.Domain.Enums;
using NovaLeave.Infrastructure.Persistence;
using NovaLeave.IntegrationTests.Support;
using NovaLeave.IntegrationTests.UseCases;

namespace NovaLeave.IntegrationTests.Concurrency;

public sealed class ApprovalConcurrencyTests
{
    [Fact]
    public async Task Approval_And_Rejection_Race_Has_One_Winner_And_One_Balance_Effect()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await ApproverTestData.SeedUserAndApproverAsync(factory);
        var client = factory.CreateClient();
        var requestId = await ApproverTestData.CreatePendingRequestAsync(factory, client);
        var rowVersion = ApproverTestData.RowVersionFor(factory, requestId);

        var approve = client.SendAsync(ApproverTestData.ApproverPost($"/aprobaciones/{requestId}/aprobar", ApproverTestData.Form(("RowVersion", rowVersion))));
        var reject = client.SendAsync(ApproverTestData.ApproverPost($"/aprobaciones/{requestId}/rechazar", ApproverTestData.Form(("RowVersion", rowVersion), ("RejectionReason", "No cumple con la politica interna."))));
        var responses = await Task.WhenAll(approve, reject);

        Assert.Contains(responses, response => response.StatusCode == HttpStatusCode.Redirect);
        Assert.Contains(responses, response => response.StatusCode == HttpStatusCode.Conflict);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        var request = db.VacationRequests.Single(request => request.Id == requestId);
        Assert.True(request.Status is RequestStatus.Approved or RequestStatus.Rejected);
        Assert.Equal(1, db.BalanceMovements.Count(movement => movement.RequestId == requestId && (movement.Type == MovementType.Deduction || movement.Type == MovementType.Release)));
    }
}
