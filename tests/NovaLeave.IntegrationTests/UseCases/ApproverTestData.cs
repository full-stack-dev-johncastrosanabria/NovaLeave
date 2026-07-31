using System.Net;
using Microsoft.Extensions.DependencyInjection;
using NovaLeave.Domain.Enums;
using NovaLeave.Infrastructure.Persistence;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.UseCases;

internal static class ApproverTestData
{
    public static async Task<Guid> CreatePendingRequestAsync(NovaLeaveWebApplicationFactory factory, HttpClient client, string userId = "user-1")
    {
        var response = await client.SendAsync(IntegrationTestDatabase.AuthenticatedPost("/mis-solicitudes/crear", Form(
            ("InputMode", "dateRange"),
            ("StartDate", "2027-01-04"),
            ("EndDate", "2027-01-06"),
            ("Reason", "Vacaciones familiares de inicio de ano.")), userId));

        if (response.StatusCode != HttpStatusCode.Redirect)
        {
            throw new InvalidOperationException($"Expected request creation redirect, got {response.StatusCode}.");
        }

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        return db.VacationRequests.Single(request => request.OwnerId == userId).Id;
    }

    public static async Task SeedUserAndApproverAsync(NovaLeaveWebApplicationFactory factory, bool approverActive = true, bool canResolve = true)
    {
        await IntegrationTestDatabase.ResetAsync(factory);
        await IntegrationTestDatabase.SeedUserAsync(factory, "user-1", "user1@example.test", 10);
        await IntegrationTestDatabase.SeedUserAsync(factory, "approver-1", "approver1@example.test", 0, approverActive, canResolve, "Approver");
    }

    public static HttpRequestMessage ApproverGet(string path, string approverId = "approver-1", bool active = true, bool canResolve = true)
    {
        return IntegrationTestDatabase.AuthenticatedGet(path, approverId, "Approver", active, canResolve);
    }

    public static HttpRequestMessage ApproverPost(string path, HttpContent content, string approverId = "approver-1", bool active = true, bool canResolve = true)
    {
        return IntegrationTestDatabase.AuthenticatedPost(path, content, approverId, "Approver", active, canResolve);
    }

    public static FormUrlEncodedContent Form(params (string Key, string Value)[] values)
    {
        return new FormUrlEncodedContent(values.Select(value => new KeyValuePair<string, string>(value.Key, value.Value)));
    }

    public static string RowVersionFor(NovaLeaveWebApplicationFactory factory, Guid requestId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
        return Convert.ToBase64String(db.VacationRequests.Single(request => request.Id == requestId).RowVersion);
    }
}
