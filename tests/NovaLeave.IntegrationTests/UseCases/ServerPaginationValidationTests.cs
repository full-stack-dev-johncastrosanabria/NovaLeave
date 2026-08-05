using Microsoft.Extensions.DependencyInjection;
using NovaLeave.Application.Common.Models;
using NovaLeave.Application.HR.Audit;
using NovaLeave.Application.HR.Balances;
using NovaLeave.Application.HR.Requests;
using NovaLeave.Domain.Entities;
using NovaLeave.Domain.ValueObjects;
using NovaLeave.Infrastructure.Identity;
using NovaLeave.Infrastructure.Persistence;
using NovaLeave.IntegrationTests.Support;

namespace NovaLeave.IntegrationTests.UseCases;

public sealed class ServerPaginationValidationTests
{
    [Fact]
    public async Task HR_Handlers_Enforce_Bounded_Pagination_Without_The_Frontend()
    {
        await using var factory = new NovaLeaveWebApplicationFactory();
        await IntegrationTestDatabase.ResetAsync(factory);

        using (var seedScope = factory.Services.CreateScope())
        {
            var db = seedScope.ServiceProvider.GetRequiredService<NovaLeaveDbContext>();
            for (var index = 1; index <= 201; index++)
            {
                var ownerId = $"pagination-user-{index:000}";
                var entityId = Guid.NewGuid();
                db.Users.Add(new ApplicationUser
                {
                    Id = ownerId,
                    UserName = $"{ownerId}@example.test",
                    Email = $"{ownerId}@example.test",
                    NormalizedUserName = $"{ownerId.ToUpperInvariant()}@EXAMPLE.TEST",
                    NormalizedEmail = $"{ownerId.ToUpperInvariant()}@EXAMPLE.TEST",
                    EmailConfirmed = true,
                    IsActive = true,
                    EmploymentStartDate = new DateOnly(2026, 1, 1)
                });
                db.VacationBalances.Add(VacationBalance.Create(ownerId, DateTimeOffset.UtcNow));
                db.VacationRequests.Add(VacationRequest.Create(
                    ownerId,
                    new DateRange(new DateOnly(2027, 4, 1), new DateOnly(2027, 4, 1)),
                    WorkingDayCount.From(1),
                    "Vacaciones planificadas para validar paginación.",
                    DateTimeOffset.UtcNow.AddMinutes(index)));
                db.AuditRecords.Add(AuditRecord.Create(
                    "system",
                    "System",
                    "PaginationTest",
                    "VacationRequest",
                    entityId,
                    "Success",
                    Guid.NewGuid(),
                    entityId,
                    null,
                    DateTimeOffset.UtcNow.AddMinutes(index)));
            }

            await db.SaveChangesAsync();
        }

        using var scope = factory.Services.CreateScope();
        var requests = await scope.ServiceProvider.GetRequiredService<GetHRRequestListQueryHandler>()
            .HandleAsync(new GetHRRequestListQuery(-10, int.MaxValue), CancellationToken.None);
        var balances = await scope.ServiceProvider.GetRequiredService<GetHRBalancesQueryHandler>()
            .HandleAsync(new GetHRBalancesQuery(-10, int.MaxValue), CancellationToken.None);
        var audit = await scope.ServiceProvider.GetRequiredService<GetHRAuditLogQueryHandler>()
            .HandleAsync(new GetHRAuditLogQuery(-10, int.MaxValue), CancellationToken.None);

        AssertBounded(requests);
        AssertBounded(balances);
        AssertBounded(audit);

        var lastPage = await scope.ServiceProvider.GetRequiredService<GetHRAuditLogQueryHandler>()
            .HandleAsync(new GetHRAuditLogQuery(int.MaxValue, PaginationParameters.DefaultPageSize), CancellationToken.None);

        Assert.Equal(5, lastPage.Page);
        Assert.Single(lastPage.Items);
        Assert.True(lastPage.HasPreviousPage);
        Assert.False(lastPage.HasNextPage);
    }

    private static void AssertBounded<T>(PagedResult<T> result)
    {
        Assert.Equal(1, result.Page);
        Assert.Equal(PaginationParameters.MaxPageSize, result.PageSize);
        Assert.Equal(201, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(200, result.Items.Count);
        Assert.False(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }
}
