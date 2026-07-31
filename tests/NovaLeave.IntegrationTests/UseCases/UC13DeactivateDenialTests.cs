using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NovaLeave.Domain.Enums;
using NovaLeave.Infrastructure.Identity;
using NovaLeave.Infrastructure.Persistence;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.UseCases;

public sealed class UC13DeactivateDenialTests
{
    [Fact]
    public async Task Deactivate_Denies_PostStart_Duplicate_And_Partial_Attempts_Without_Balance_Effects()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await ApproverTestData.SeedUserAndApproverAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "user-2", "user2@example.test", 10);
        await IntegrationTestDatabase.SeedUserAsync(factory, "user-3", "user3@example.test", 10);
        var client = factory.CreateClient();
        var postStartId = await UC13DeactivateApprovedRequestTests.CreateApprovedRequestAsync(factory, client);
        await MoveRequestDatesAsync(factory, postStartId, new DateOnly(2026, 1, 5), new DateOnly(2026, 1, 7));
        var postStartRowVersion = ApproverTestData.RowVersionFor(factory, postStartId);

        var postStart = await client.SendAsync(ApproverTestData.ApproverPost(
            $"/aprobaciones/{postStartId}/desactivar",
            ApproverTestData.Form(("RowVersion", postStartRowVersion))));

        var partialId = await UC13DeactivateApprovedRequestTests.CreateApprovedRequestAsync(factory, client, "user-2");
        var partialRowVersion = ApproverTestData.RowVersionFor(factory, partialId);
        var partial = await client.SendAsync(ApproverTestData.ApproverPost(
            $"/aprobaciones/{partialId}/desactivar",
            ApproverTestData.Form(
                ("RowVersion", partialRowVersion),
                ("StartDate", "2027-01-04"),
                ("EndDate", "2027-01-05"))));

        var duplicateId = await UC13DeactivateApprovedRequestTests.CreateApprovedRequestAsync(factory, client, "user-3");
        var duplicateRowVersion = ApproverTestData.RowVersionFor(factory, duplicateId);
        var first = await client.SendAsync(ApproverTestData.ApproverPost(
            $"/aprobaciones/{duplicateId}/desactivar",
            ApproverTestData.Form(("RowVersion", duplicateRowVersion))));
        var second = await client.SendAsync(ApproverTestData.ApproverPost(
            $"/aprobaciones/{duplicateId}/desactivar",
            ApproverTestData.Form(("RowVersion", duplicateRowVersion))));

        Assert.Equal(HttpStatusCode.Conflict, postStart.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, partial.StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        Assert.Equal(RequestStatus.Approved, db.VacationRequests.Single(request => request.Id == postStartId).Status);
        Assert.Equal(RequestStatus.CancelledByApprover, db.VacationRequests.Single(request => request.Id == duplicateId).Status);
        Assert.Single(db.BalanceMovements.Where(movement => movement.RequestId == duplicateId && movement.Type == MovementType.Restoration));
        Assert.DoesNotContain(db.BalanceMovements, movement => movement.RequestId == postStartId && movement.Type == MovementType.Restoration);
        Assert.DoesNotContain(db.BalanceMovements, movement => movement.RequestId == partialId && movement.Type == MovementType.Restoration);
    }

    [Theory]
    [InlineData("user-1", "User", true, false, HttpStatusCode.Forbidden)]
    [InlineData("hr-1", "HR", true, false, HttpStatusCode.Forbidden)]
    [InlineData("user-1", "User,Approver", true, true, HttpStatusCode.NotFound)]
    [InlineData("approver-1", "Approver", false, true, HttpStatusCode.Forbidden)]
    [InlineData("approver-1", "Approver", true, false, HttpStatusCode.Forbidden)]
    public async Task Deactivate_Denies_User_HR_Self_Inactive_And_Disabled_Approver_Attempts(
        string actorId,
        string roles,
        bool isActive,
        bool canResolve,
        HttpStatusCode expected)
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await ApproverTestData.SeedUserAndApproverAsync(factory);
        if (roles.Contains("Approver", StringComparison.Ordinal) && actorId == "user-1")
        {
            await AddApproverRoleAsync(factory, actorId);
            await UC13DeactivateApprovedRequestTests.SetApproverCapabilityAsync(factory, actorId, canResolve: true);
        }

        if (roles.Contains("HR", StringComparison.Ordinal))
        {
            await IntegrationTestDatabase.SeedUserAsync(factory, "hr-1", "hr@example.test", 0, roles: "HR");
        }

        var client = factory.CreateClient();
        var requestId = actorId == "user-1"
            ? await UC13DeactivateApprovedRequestTests.CreateApprovedRequestAsync(factory, client, "user-1")
            : await UC13DeactivateApprovedRequestTests.CreateApprovedRequestAsync(factory, client);
        var rowVersion = ApproverTestData.RowVersionFor(factory, requestId);

        var response = await client.SendAsync(IntegrationTestDatabase.AuthenticatedPost(
            $"/aprobaciones/{requestId}/desactivar",
            ApproverTestData.Form(("RowVersion", rowVersion)),
            actorId,
            roles,
            isActive,
            canResolve));

        Assert.Equal(expected, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        Assert.Equal(RequestStatus.Approved, db.VacationRequests.Single(request => request.Id == requestId).Status);
        Assert.DoesNotContain(db.BalanceMovements, movement => movement.RequestId == requestId && movement.Type == MovementType.Restoration);
    }

    private static async Task MoveRequestDatesAsync(NovaLeaveWebApplicationFactory factory, Guid requestId, DateOnly startDate, DateOnly endDate)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE VacationRequest SET StartDate = {startDate}, EndDate = {endDate} WHERE Id = {requestId}");
    }

    private static async Task AddApproverRoleAsync(NovaLeaveWebApplicationFactory factory, string userId)
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByIdAsync(userId);
        Assert.NotNull(user);
        await userManager.AddToRoleAsync(user!, "Approver");
    }
}
