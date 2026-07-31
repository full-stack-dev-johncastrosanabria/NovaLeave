using Microsoft.Extensions.DependencyInjection;
using NovaLeave.Application.System.CancelTimedOutRequests;
using NovaLeave.Domain.Enums;
using NovaLeave.Infrastructure.Persistence;
using NovaLeave.IntegrationTests.Support;
using NovaLeave.IntegrationTests.UseCases;

namespace NovaLeave.IntegrationTests.Idempotency;

public sealed class TimeoutIdempotencyTests
{
    [Fact]
    public async Task Repeated_Timeout_Run_Releases_And_Audits_Exactly_Once()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await ApproverTestData.SeedUserAndApproverAsync(factory);
        var client = factory.CreateClient();
        var requestId = await ApproverTestData.CreatePendingRequestAsync(factory, client);
        await UC16TimeoutCancellationTests.AgeRequestAsync(factory, requestId, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        using var scope = factory.Services.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<CancelTimedOutRequestsHandler>();

        var first = await handler.HandleAsync(new CancelTimedOutRequestsCommand(new DateOnly(2026, 1, 15)), CancellationToken.None);
        var second = await handler.HandleAsync(new CancelTimedOutRequestsCommand(new DateOnly(2026, 1, 15)), CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        Assert.Equal(RequestStatus.CancelledByTimeout, db.VacationRequests.Single(request => request.Id == requestId).Status);
        Assert.Single(db.BalanceMovements.Where(movement => movement.RequestId == requestId && movement.Type == MovementType.Release));
        Assert.Single(db.AuditRecords.Where(audit => audit.EntityId == requestId && audit.Action == "Timeout"));
    }
}
