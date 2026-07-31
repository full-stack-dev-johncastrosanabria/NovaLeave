using System.Net;
using Microsoft.Extensions.DependencyInjection;
using NovaLeave.Application.System.CancelTimedOutRequests;
using NovaLeave.Domain.Enums;
using NovaLeave.Infrastructure.Persistence;
using NovaLeave.IntegrationTests.Support;
using NovaLeave.IntegrationTests.UseCases;

namespace NovaLeave.IntegrationTests.Concurrency;

public sealed class TimeoutApprovalRaceTests
{
    [Fact]
    public async Task Timeout_And_Approval_Race_Has_One_Winner_And_One_Balance_Effect()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await ApproverTestData.SeedUserAndApproverAsync(factory);
        var client = factory.CreateClient();
        var requestId = await ApproverTestData.CreatePendingRequestAsync(factory, client);
        await UC16TimeoutCancellationTests.AgeRequestAsync(factory, requestId, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var rowVersion = ApproverTestData.RowVersionFor(factory, requestId);

        var approval = client.SendAsync(ApproverTestData.ApproverPost(
            $"/aprobaciones/{requestId}/aprobar",
            ApproverTestData.Form(("RowVersion", rowVersion))));
        var timeout = RunTimeoutAsync(factory);
        await Task.WhenAll(approval, timeout);
        var approvalResponse = await approval;
        var timeoutResult = await timeout;

        Assert.True(approvalResponse.StatusCode is HttpStatusCode.Redirect or HttpStatusCode.Conflict);
        Assert.True(timeoutResult.IsSuccess);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        var request = db.VacationRequests.Single(request => request.Id == requestId);
        Assert.True(request.Status is RequestStatus.Approved or RequestStatus.CancelledByTimeout);
        Assert.Equal(1, db.BalanceMovements.Count(movement =>
            movement.RequestId == requestId &&
            (movement.Type == MovementType.Deduction || movement.Type == MovementType.Release)));
        Assert.Equal(1, db.AuditRecords.Count(audit =>
            audit.EntityId == requestId &&
            (audit.Action == "Approve" || audit.Action == "Timeout")));
    }

    private static async Task<NovaLeave.Application.Common.Results.Result> RunTimeoutAsync(NovaLeaveWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<CancelTimedOutRequestsHandler>();
        return await handler.HandleAsync(new CancelTimedOutRequestsCommand(new DateOnly(2026, 1, 15)), CancellationToken.None);
    }
}
