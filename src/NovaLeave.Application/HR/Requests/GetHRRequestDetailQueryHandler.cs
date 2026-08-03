using NovaLeave.Application.Common.Errors;
using NovaLeave.Application.Common.Interfaces;
using NovaLeave.Application.Common.Results;
using NovaLeave.Domain.Entities;
using NovaLeave.Domain.Enums;

namespace NovaLeave.Application.HR.Requests;

public sealed record HRAuditTrailItem(DateTime TimestampUtc, string ActorId, string ActorRole, string Action, string Result, string? Data);

public sealed record HRRequestDetail(
    Guid Id,
    string RequesterId,
    string RequesterName,
    DateOnly StartDate,
    DateOnly EndDate,
    int WorkingDays,
    RequestStatus Status,
    string Reason,
    string? RejectionReason,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyList<HRAuditTrailItem> AuditTrail);

public sealed class GetHRRequestDetailQueryHandler
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserDirectory _userDirectory;
    private readonly TimeProvider _timeProvider;

    public GetHRRequestDetailQueryHandler(IApplicationDbContext dbContext, IUserDirectory userDirectory, TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _userDirectory = userDirectory;
        _timeProvider = timeProvider;
    }

    public async Task<Result<HRRequestDetail>> HandleAsync(string hrUserId, Guid requestId, CancellationToken cancellationToken)
    {
        var request = _dbContext.VacationRequests.SingleOrDefault(candidate => candidate.Id == requestId);
        if (request is null)
        {
            return Result<HRRequestDetail>.Failure(new Error(ErrorCodes.NotFound, "Solicitud no encontrada."));
        }

        AuditSensitiveAccess(hrUserId, request);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var users = await _userDirectory.GetUsersByIdsAsync([request.OwnerId], cancellationToken);
        var auditTrail = _dbContext.AuditRecords
            .Where(record => record.EntityType == nameof(VacationRequest) && record.EntityId == request.Id)
            .OrderByDescending(record => record.TimestampUtc)
            .Select(record => new HRAuditTrailItem(record.TimestampUtc, record.ActorId, record.ActorRole, record.Action, record.Result, record.Data))
            .ToList();

        return Result<HRRequestDetail>.Success(new HRRequestDetail(
            request.Id,
            request.OwnerId,
            users.TryGetValue(request.OwnerId, out var user) ? user.DisplayName : request.OwnerId,
            request.StartDate,
            request.EndDate,
            request.WorkingDays,
            request.Status,
            request.Reason,
            request.RejectionReason,
            request.CreatedAtUtc,
            request.UpdatedAtUtc,
            auditTrail));
    }

    private void AuditSensitiveAccess(string hrUserId, VacationRequest request)
    {
        AddSensitiveAccessAudit(hrUserId, request.Id, "RequestReason");
        if (!string.IsNullOrWhiteSpace(request.RejectionReason))
        {
            AddSensitiveAccessAudit(hrUserId, request.Id, "RequestRejectionReason");
        }
    }

    private void AddSensitiveAccessAudit(string hrUserId, Guid requestId, string fieldName)
    {
        var correlationId = Guid.NewGuid();
        _dbContext.AddAuditRecord(AuditRecord.Create(
            hrUserId,
            "HR",
            "HRSensitiveAccess",
            nameof(VacationRequest),
            requestId,
            "Success",
            correlationId,
            requestId,
            $$"""{"Field":"{{fieldName}}"}""",
            _timeProvider.GetUtcNow()));
    }
}
