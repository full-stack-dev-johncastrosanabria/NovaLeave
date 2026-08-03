using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NovaLeave.Application.Common.Errors;
using NovaLeave.Application.Common.Results;
using NovaLeave.Application.HR.ApproverCapabilities;
using NovaLeave.Domain.Entities;
using NovaLeave.Infrastructure.Persistence;

namespace NovaLeave.Infrastructure.Identity;

public sealed class ApproverCapabilityStore : IApproverCapabilityStore
{
    private readonly NovaLeaveDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TimeProvider _timeProvider;

    public ApproverCapabilityStore(NovaLeaveDbContext dbContext, UserManager<ApplicationUser> userManager, TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _timeProvider = timeProvider;
    }

    public async Task<IReadOnlyList<ApproverCapabilityItem>> ListApproversAsync(CancellationToken cancellationToken)
    {
        var users = await _userManager.GetUsersInRoleAsync("Approver");
        return users
            .OrderBy(user => user.Email)
            .Select(ToItem)
            .ToList();
    }

    public async Task<ApproverCapabilityItem?> GetApproverAsync(string targetUserId, CancellationToken cancellationToken)
    {
        var user = await _userManager.Users.SingleOrDefaultAsync(candidate => candidate.Id == targetUserId, cancellationToken);
        if (user is null || !await _userManager.IsInRoleAsync(user, "Approver"))
        {
            return null;
        }

        return ToItem(user);
    }

    public async Task<Result> ToggleAsync(ToggleApproverCapabilityRequest request, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        var hr = await _userManager.Users.SingleOrDefaultAsync(candidate => candidate.Id == request.HRUserId, cancellationToken);
        if (hr is null || !hr.IsActive || !await _userManager.IsInRoleAsync(hr, "HR"))
        {
            AddAudit(request, false, null, "Forbidden", "InactiveOrNonHRActor");
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result.Failure(Error.Forbidden("RRHH activo requerido."));
        }

        var user = await _userManager.Users.SingleOrDefaultAsync(candidate => candidate.Id == request.TargetUserId, cancellationToken);
        if (user is null || !await _userManager.IsInRoleAsync(user, "Approver"))
        {
            AddAudit(request, false, null, "NotFound", "TargetNotApprover");
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result.Failure(new Error(ErrorCodes.NotFound, "Aprobador no encontrado."));
        }

        if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Trim().Length is < 10 or > 500)
        {
            AddAudit(request, false, user.CanResolveRequests, "Validation", "MissingJustification");
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result.Failure(Error.Validation("La justificacion es requerida y debe tener entre 10 y 500 caracteres."));
        }

        if (!request.Confirmed)
        {
            AddAudit(request, false, user.CanResolveRequests, "Validation", "MissingConfirmation");
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result.Failure(Error.Validation("La confirmacion explicita es requerida."));
        }

        if (!user.RowVersion.SequenceEqual(request.ExpectedRowVersion))
        {
            AddAudit(request, false, user.CanResolveRequests, "Conflict", "StaleRowVersion");
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result.Failure(Error.Conflict("La capacidad cambio. Actualice e intente de nuevo."));
        }

        var before = user.CanResolveRequests;
        user.CanResolveRequests = request.Enable;
        var update = await _userManager.UpdateAsync(user);
        if (!update.Succeeded)
        {
            AddAudit(request, false, before, "Conflict", "IdentityUpdateFailed");
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result.Failure(Error.Conflict("No fue posible actualizar la capacidad."));
        }

        AddAudit(request, true, before, "Success", null);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Result.Success();
    }

    private static ApproverCapabilityItem ToItem(ApplicationUser user)
    {
        return new ApproverCapabilityItem(
            user.Id,
            user.Email ?? user.UserName ?? user.Id,
            user.IsActive,
            user.CanResolveRequests,
            user.RowVersion);
    }

    private void AddAudit(ToggleApproverCapabilityRequest request, bool success, bool? before, string result, string? failure)
    {
        var data = success
            ? $$"""{"TargetUserId":"{{request.TargetUserId}}","Before":{{JsonBool(before ?? false)}},"After":{{JsonBool(request.Enable)}},"Justification":"{{Escape(request.Reason.Trim())}}","RowVersion":"{{Convert.ToBase64String(request.ExpectedRowVersion)}}"}"""
            : $$"""{"TargetUserId":"{{request.TargetUserId}}","Attempted":{{JsonBool(request.Enable)}},"Failure":"{{failure}}","RowVersion":"{{Convert.ToBase64String(request.ExpectedRowVersion)}}"}""";

        _dbContext.AddAuditRecord(AuditRecord.Create(
            request.HRUserId,
            "HR",
            success ? "ToggleCapability" : "ToggleCapabilityFailed",
            nameof(ApplicationUser),
            Guid.Empty,
            result,
            Guid.NewGuid(),
            Guid.NewGuid(),
            data,
            _timeProvider.GetUtcNow()));
    }

    private static string JsonBool(bool value)
    {
        return value ? "true" : "false";
    }

    private static string Escape(string value)
    {
        return value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);
    }
}
